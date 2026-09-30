import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";

import {
  Button,
  Card,
  Col,
  DatePicker,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
  message,
} from "antd";

import {
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
  LeftOutlined,
  RightOutlined,
  EditOutlined,
} from "@ant-design/icons";

import dayjs from "dayjs";

import {
  getExpenses,
} from "../../api/expensesApi";

const { Title, Text } = Typography;

function getValue(record, ...keys) {
  for (const key of keys) {
    if (
      record &&
      record[key] !== undefined &&
      record[key] !== null
    ) {
      return record[key];
    }
  }

  return null;
}

function formatAmount(value) {
  return Number(value || 0).toLocaleString(
    undefined,
    {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }
  );
}

export default function Expenses({
  onNew,
  onEdit,
}) {
  const [messageApi, contextHolder] =
    message.useMessage();

  const [expenses, setExpenses] =
    useState([]);

  const [loading, setLoading] =
    useState(false);

  const [searchText, setSearchText] =
    useState("");

  const [dateFilter, setDateFilter] =
    useState("today");

  const [customRange, setCustomRange] =
    useState(null);

  const [selectedDate, setSelectedDate] =
    useState(dayjs());

  const [statusFilter, setStatusFilter] =
    useState("all");

  const loadExpenses = useCallback(
    async () => {
      try {
        setLoading(true);

        const data = await getExpenses();

        setExpenses(
          Array.isArray(data)
            ? data
            : []
        );
      } catch (error) {
        console.error(error);

        messageApi.error(
          error?.response?.data?.message ||
            "Failed to load expenses."
        );
      } finally {
        setLoading(false);
      }
    },
    [messageApi]
  );

  /*
   * Defer the initial API load so React does not
   * treat the synchronous state updates inside
   * loadExpenses() as cascading renders.
   */
  useEffect(() => {
    const timer = setTimeout(() => {
      loadExpenses();
    }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [loadExpenses]);

  const filteredExpenses = useMemo(() => {
    let result = [...expenses];

    const search =
      searchText.trim().toLowerCase();

    if (search) {
      result = result.filter((item) => {
        const expenseNumber =
          String(
            getValue(
              item,
              "expenseNumber",
              "ExpenseNumber"
            ) || ""
          ).toLowerCase();

        const payee =
          String(
            getValue(
              item,
              "payeeName",
              "PayeeName"
            ) || ""
          ).toLowerCase();

        const paymentAccount =
          String(
            getValue(
              item,
              "paymentAccountName",
              "PaymentAccountName"
            ) || ""
          ).toLowerCase();

        return (
          expenseNumber.includes(search) ||
          payee.includes(search) ||
          paymentAccount.includes(search)
        );
      });
    }

    if (statusFilter !== "all") {
      result = result.filter((item) => {
        const status =
          getValue(
            item,
            "statusCode",
            "StatusCode"
          ) || "";

        return (
          String(status).toUpperCase() ===
          statusFilter
        );
      });
    }

    if (dateFilter !== "all") {
      result = result.filter((item) => {
        const rawDate =
          getValue(
            item,
            "expenseDate",
            "ExpenseDate"
          );

        if (!rawDate) {
          return false;
        }

        const expenseDate =
          dayjs(rawDate);

        if (!expenseDate.isValid()) {
          return false;
        }

        if (dateFilter === "today") {
          return expenseDate.isSame(
            dayjs(),
            "day"
          );
        }

        if (dateFilter === "yesterday") {
          return expenseDate.isSame(
            dayjs().subtract(1, "day"),
            "day"
          );
        }

        if (dateFilter === "thisWeek") {
          return expenseDate.isSame(
            dayjs(),
            "week"
          );
        }

        if (dateFilter === "thisMonth") {
          return expenseDate.isSame(
            dayjs(),
            "month"
          );
        }

        if (dateFilter === "thisYear") {
          return expenseDate.isSame(
            dayjs(),
            "year"
          );
        }

        if (
          dateFilter === "custom" &&
          customRange?.length === 2
        ) {
          return (
            expenseDate.isSame(
              customRange[0],
              "day"
            ) ||
            expenseDate.isSame(
              customRange[1],
              "day"
            ) ||
            (
              expenseDate.isAfter(
                customRange[0],
                "day"
              ) &&
              expenseDate.isBefore(
                customRange[1],
                "day"
              )
            )
          );
        }

        return true;
      });
    }

    return result;
  }, [
    expenses,
    searchText,
    dateFilter,
    customRange,
    statusFilter,
  ]);

  const totalExpense = useMemo(
    () =>
      filteredExpenses.reduce(
        (sum, item) =>
          sum +
          Number(
            getValue(
              item,
              "totalAmount",
              "TotalAmount"
            ) || 0
          ),
        0
      ),
    [filteredExpenses]
  );

  const postedTotal = useMemo(
    () =>
      filteredExpenses
        .filter((item) => {
          const status =
            getValue(
              item,
              "statusCode",
              "StatusCode"
            ) || "";

          return (
            String(status).toUpperCase() ===
            "POSTED"
          );
        })
        .reduce(
          (sum, item) =>
            sum +
            Number(
              getValue(
                item,
                "totalAmount",
                "TotalAmount"
              ) || 0
            ),
          0
        ),
    [filteredExpenses]
  );

  const draftCount = useMemo(
    () =>
      filteredExpenses.filter((item) => {
        const status =
          getValue(
            item,
            "statusCode",
            "StatusCode"
          ) || "";

        return (
          String(status).toUpperCase() ===
          "DRAFT"
        );
      }).length,
    [filteredExpenses]
  );

  const cancelledCount = useMemo(
    () =>
      filteredExpenses.filter((item) => {
        const status =
          getValue(
            item,
            "statusCode",
            "StatusCode"
          ) || "";

        return (
          String(status).toUpperCase() ===
          "CANCELLED"
        );
      }).length,
    [filteredExpenses]
  );

  const handlePreviousDay = () => {
    const previous =
      selectedDate.subtract(1, "day");

    setSelectedDate(previous);
    setDateFilter("custom");

    setCustomRange([
      previous.startOf("day"),
      previous.endOf("day"),
    ]);
  };

  const handleNextDay = () => {
    const next =
      selectedDate.add(1, "day");

    setSelectedDate(next);
    setDateFilter("custom");

    setCustomRange([
      next.startOf("day"),
      next.endOf("day"),
    ]);
  };

  const handleDateFilterChange = (value) => {
    setDateFilter(value);

    if (value !== "custom") {
      setCustomRange(null);
    }
  };

  const handleCustomRangeChange = (dates) => {
    if (!dates) {
      setCustomRange(null);
      return;
    }

    setCustomRange(dates);
    setDateFilter("custom");
  };

  const getStatusTag = (record) => {
    const status =
      String(
        getValue(
          record,
          "statusCode",
          "StatusCode"
        ) || ""
      ).toUpperCase();

    if (status === "POSTED") {
      return (
        <Tag color="green">
          POSTED
        </Tag>
      );
    }

    if (status === "CANCELLED") {
      return (
        <Tag color="red">
          CANCELLED
        </Tag>
      );
    }

    return (
      <Tag color="orange">
        DRAFT
      </Tag>
    );
  };

  const columns = [
    {
      title: "Voucher No.",
      key: "expenseNumber",
      width: 150,
      render: (_, record) =>
        getValue(
          record,
          "expenseNumber",
          "ExpenseNumber"
        ) || "-",
    },
    {
      title: "Date",
      key: "expenseDate",
      width: 120,
      render: (_, record) => {
        const value =
          getValue(
            record,
            "expenseDate",
            "ExpenseDate"
          );

        return value
          ? dayjs(value).format(
              "DD/MM/YYYY"
            )
          : "-";
      },
    },
    {
      title: "Payee",
      key: "payeeName",
      width: 180,
      render: (_, record) =>
        getValue(
          record,
          "payeeName",
          "PayeeName"
        ) || "-",
    },
    {
      title: "Payment Account",
      key: "paymentAccount",
      width: 220,
      render: (_, record) => {
        const code =
          getValue(
            record,
            "paymentAccountCode",
            "PaymentAccountCode"
          );

        const name =
          getValue(
            record,
            "paymentAccountName",
            "PaymentAccountName"
          );

        if (!code && !name) {
          return "-";
        }

        return (
          <span>
            {code ? `${code} - ` : ""}
            {name || ""}
          </span>
        );
      },
    },
    {
      title: "Currency",
      key: "currencyId",
      width: 100,
      render: (_, record) =>
        getValue(
          record,
          "currencyId",
          "CurrencyId"
        ) || "-",
    },
    {
      title: "Total",
      key: "totalAmount",
      width: 140,
      align: "right",
      render: (_, record) =>
        formatAmount(
          getValue(
            record,
            "totalAmount",
            "TotalAmount"
          )
        ),
    },
    {
      title: "Status",
      key: "status",
      width: 120,
      render: (_, record) =>
        getStatusTag(record),
    },
    {
      title: "Edit",
      key: "edit",
      width: 80,
      align: "center",
      render: (_, record) => (
        <Button
          type="text"
          icon={<EditOutlined />}
          onClick={() => {
            const id =
              getValue(
                record,
                "expenseId",
                "ExpenseId"
              );

            if (id) {
              onEdit(id);
            }
          }}
        />
      ),
    },
  ];

  return (
    <>
      {contextHolder}

      <div
        style={{
          marginBottom: 24,
        }}
      >
        <Row
          justify="space-between"
          align="middle"
          gutter={[16, 16]}
        >
          <Col>
            <Title
              level={2}
              style={{
                margin: 0,
              }}
            >
              Expenses
            </Title>

            <Text type="secondary">
              Expense management
            </Text>
          </Col>

          <Col>
            <Space>
              <Button
                icon={<ReloadOutlined />}
                loading={loading}
                onClick={loadExpenses}
              >
                Refresh
              </Button>

              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={onNew}
              >
                New Expense
              </Button>
            </Space>
          </Col>
        </Row>
      </div>

      <Row
        gutter={[16, 16]}
        style={{
          marginBottom: 16,
        }}
      >
        <Col
          xs={24}
          sm={12}
          lg={6}
        >
          <Card>
            <Statistic
              title="Expense Total"
              value={totalExpense}
              precision={2}
            />
          </Card>
        </Col>

        <Col
          xs={24}
          sm={12}
          lg={6}
        >
          <Card>
            <Statistic
              title="Posted Total"
              value={postedTotal}
              precision={2}
            />
          </Card>
        </Col>

        <Col
          xs={24}
          sm={12}
          lg={6}
        >
          <Card>
            <Statistic
              title="Draft"
              value={draftCount}
            />
          </Card>
        </Col>

        <Col
          xs={24}
          sm={12}
          lg={6}
        >
          <Card>
            <Statistic
              title="Cancelled"
              value={cancelledCount}
            />
          </Card>
        </Col>
      </Row>

      <Card>
        <Space
          wrap
          style={{
            width: "100%",
            marginBottom: 16,
          }}
        >
          <Input
            allowClear
            prefix={<SearchOutlined />}
            placeholder="Search voucher, payee..."
            value={searchText}
            onChange={(event) =>
              setSearchText(
                event.target.value
              )
            }
            style={{
              width: 260,
            }}
          />

          <Select
            value={dateFilter}
            onChange={
              handleDateFilterChange
            }
            style={{
              width: 150,
            }}
            options={[
              {
                value: "all",
                label: "All Dates",
              },
              {
                value: "today",
                label: "Today",
              },
              {
                value: "yesterday",
                label: "Yesterday",
              },
              {
                value: "thisWeek",
                label: "This Week",
              },
              {
                value: "thisMonth",
                label: "This Month",
              },
              {
                value: "thisYear",
                label: "This Year",
              },
              {
                value: "custom",
                label: "Custom",
              },
            ]}
          />

          {dateFilter === "custom" && (
            <DatePicker.RangePicker
              value={customRange}
              onChange={
                handleCustomRangeChange
              }
            />
          )}

          <Button
            icon={<LeftOutlined />}
            onClick={handlePreviousDay}
          />

          <DatePicker
            value={selectedDate}
            onChange={(date) => {
              if (!date) {
                return;
              }

              setSelectedDate(date);
              setDateFilter("custom");

              setCustomRange([
                date.startOf("day"),
                date.endOf("day"),
              ]);
            }}
            format="DD/MM/YYYY"
          />

          <Button
            icon={<RightOutlined />}
            onClick={handleNextDay}
          />

          <Select
            value={statusFilter}
            onChange={setStatusFilter}
            style={{
              width: 140,
            }}
            options={[
              {
                value: "all",
                label: "All Status",
              },
              {
                value: "DRAFT",
                label: "Draft",
              },
              {
                value: "POSTED",
                label: "Posted",
              },
              {
                value: "CANCELLED",
                label: "Cancelled",
              },
            ]}
          />
        </Space>

        <Table
          rowKey={(record) =>
            getValue(
              record,
              "expenseId",
              "ExpenseId"
            )
          }
          loading={loading}
          columns={columns}
          dataSource={filteredExpenses}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
          }}
          scroll={{
            x: 1150,
          }}
          onRow={(record) => ({
            onDoubleClick: () => {
              const id =
                getValue(
                  record,
                  "expenseId",
                  "ExpenseId"
                );

              if (id) {
                onEdit(id);
              }
            },
          })}
        />
      </Card>
    </>
  );
}
