import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Col,
  DatePicker,
  Divider,
  Input,
  Row,
  Select,
  Space,
  Table,
  Tag,
  Typography,
  message,
} from "antd";
import {
  LeftOutlined,
  PlusOutlined,
  ReloadOutlined,
  RightOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";

import { getPurchaseInvoices } from "../../api/purchaseInvoicesApi";

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

function getValue(obj, ...keys) {
  for (const key of keys) {
    if (obj?.[key] !== undefined && obj?.[key] !== null) {
      return obj[key];
    }
  }

  return null;
}

function formatNumber(value) {
  return Number(value || 0).toLocaleString(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  });
}

function getInvoiceDate(value) {
  if (!value) {
    return null;
  }

  const date = dayjs(value);

  return date.isValid() ? date : null;
}

/*
 * Convert invoice total to MMK for summary only.
 *
 * CurrencyId:
 * 1 = THB
 * 2 = MMK
 * 3 = USD
 *
 * ExchangeRate:
 * 1 Invoice Currency = X MMK
 */
function getMmkTotal(invoice) {
  const total = Number(
    getValue(
      invoice,
      "totalAmount",
      "TotalAmount"
    ) || 0
  );

  const currencyId = Number(
    getValue(
      invoice,
      "currencyId",
      "CurrencyId"
    ) || 0
  );

  const exchangeRate = Number(
    getValue(
      invoice,
      "exchangeRate",
      "ExchangeRate"
    ) || 1
  );

  // MMK
  if (currencyId === 2) {
    return total;
  }

  // Other currencies -> MMK
  return total * exchangeRate;
}

export default function PurchaseInvoices({
  onNewPurchase,
  onEditPurchase,
}) {
  const [messageApi, contextHolder] =
    message.useMessage();

  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(false);

  const [searchText, setSearchText] = useState("");

  const [dateFilter, setDateFilter] =
    useState("today");

  const [dateRange, setDateRange] = useState([
    dayjs().startOf("day"),
    dayjs().endOf("day"),
  ]);

  const [pageSize, setPageSize] = useState(10);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);

      const data =
        await getPurchaseInvoices();

      setInvoices(
        Array.isArray(data) ? data : []
      );
    } catch (error) {
      messageApi.error(
        error?.response?.data?.message ||
          "Failed to load purchase invoices."
      );
    } finally {
      setLoading(false);
    }
  }, [messageApi]);

  useEffect(() => {
    const timer = setTimeout(() => {
      void loadData();
    }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [loadData]);

  const handleDateFilterChange = (
    value
  ) => {
    setDateFilter(value);

    const today = dayjs();

    if (value === "all") {
      setDateRange(null);
      return;
    }

    if (value === "today") {
      setDateRange([
        today.startOf("day"),
        today.endOf("day"),
      ]);
      return;
    }

    if (value === "thisWeek") {
      setDateRange([
        today.startOf("week"),
        today.endOf("week"),
      ]);
      return;
    }

    if (value === "thisMonth") {
      setDateRange([
        today.startOf("month"),
        today.endOf("month"),
      ]);
      return;
    }

    if (value === "thisYear") {
      setDateRange([
        today.startOf("year"),
        today.endOf("year"),
      ]);
    }
  };

  const handleRangeChange = (dates) => {
    if (!dates) {
      handleDateFilterChange("today");
      return;
    }

    setDateFilter("custom");
    setDateRange(dates);
  };

  const handlePreviousDay = () => {
    const currentDate =
      dateRange?.[0] || dayjs();

    const previousDay = currentDate
      .subtract(1, "day")
      .startOf("day");

    setDateFilter("custom");

    setDateRange([
      previousDay,
      previousDay.endOf("day"),
    ]);
  };

  const handleNextDay = () => {
    const currentDate =
      dateRange?.[0] || dayjs();

    const nextDay = currentDate
      .add(1, "day")
      .startOf("day");

    setDateFilter("custom");

    setDateRange([
      nextDay,
      nextDay.endOf("day"),
    ]);
  };

  const filteredInvoices = useMemo(() => {
    const search =
      searchText.trim().toLowerCase();

    const fromDate = dateRange?.[0]
      ? dayjs(dateRange[0]).startOf("day")
      : null;

    const toDate = dateRange?.[1]
      ? dayjs(dateRange[1]).endOf("day")
      : null;

    return invoices.filter((invoice) => {
      const invoiceDateValue =
        getValue(
          invoice,
          "invoiceDate",
          "InvoiceDate"
        );

      const invoiceDate =
        getInvoiceDate(
          invoiceDateValue
        );

      if (!invoiceDate) {
        return false;
      }

      if (
        fromDate &&
        invoiceDate.isBefore(fromDate)
      ) {
        return false;
      }

      if (
        toDate &&
        invoiceDate.isAfter(toDate)
      ) {
        return false;
      }

      if (!search) {
        return true;
      }

      const invoiceNumber =
        String(
          getValue(
            invoice,
            "invoiceNumber",
            "InvoiceNumber"
          ) || ""
        ).toLowerCase();

      const supplierName =
        String(
          getValue(
            invoice,
            "supplierName",
            "SupplierName"
          ) || ""
        ).toLowerCase();

      const warehouseName =
        String(
          getValue(
            invoice,
            "warehouseName",
            "WarehouseName"
          ) || ""
        ).toLowerCase();

      const currencyCode =
        String(
          getValue(
            invoice,
            "currencyCode",
            "CurrencyCode"
          ) || ""
        ).toLowerCase();

      const statusName =
        String(
          getValue(
            invoice,
            "statusName",
            "StatusName"
          ) || ""
        ).toLowerCase();

      return (
        invoiceNumber.includes(search) ||
        supplierName.includes(search) ||
        warehouseName.includes(search) ||
        currencyCode.includes(search) ||
        statusName.includes(search)
      );
    });
  }, [
    invoices,
    searchText,
    dateRange,
  ]);

  /*
   * Only POSTED invoices are included
   * in Daily Purchase Total and Grand Total.
   */
  const postedInvoices = useMemo(() => {
    return filteredInvoices.filter(
      (invoice) => {
        const statusCode =
          String(
            getValue(
              invoice,
              "statusCode",
              "StatusCode"
            ) || ""
          ).toUpperCase();

        return statusCode === "POSTED";
      }
    );
  }, [filteredInvoices]);

  /*
   * Daily Purchase Total
   * is always calculated in MMK.
   */
  const dailySummary = useMemo(() => {
    const grouped = {};

    postedInvoices.forEach((invoice) => {
      const invoiceDateValue =
        getValue(
          invoice,
          "invoiceDate",
          "InvoiceDate"
        );

      const invoiceDate =
        getInvoiceDate(
          invoiceDateValue
        );

      if (!invoiceDate) {
        return;
      }

      const dateKey =
        invoiceDate.format(
          "YYYY-MM-DD"
        );

      if (!grouped[dateKey]) {
        grouped[dateKey] = {
          date:
            invoiceDate.startOf("day"),
          invoiceCount: 0,
          totalAmountMMK: 0,
        };
      }

      grouped[dateKey].invoiceCount += 1;

      grouped[
        dateKey
      ].totalAmountMMK +=
        getMmkTotal(invoice);
    });

    return Object.values(grouped).sort(
      (a, b) =>
        b.date.valueOf() -
        a.date.valueOf()
    );
  }, [postedInvoices]);

  const postedInvoiceCount =
    postedInvoices.length;

  /*
   * Grand Total is always MMK.
   */
  const grandTotal = useMemo(() => {
    return postedInvoices.reduce(
      (sum, invoice) =>
        sum + getMmkTotal(invoice),
      0
    );
  }, [postedInvoices]);

  const columns = [
    {
      title: "Invoice No",
      key: "invoiceNumber",
      sorter: (a, b) =>
        String(
          getValue(
            a,
            "invoiceNumber",
            "InvoiceNumber"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "invoiceNumber",
              "InvoiceNumber"
            ) || ""
          )
        ),
      render: (_, record) => (
        <strong>
          {getValue(
            record,
            "invoiceNumber",
            "InvoiceNumber"
          ) || "-"}
        </strong>
      ),
    },
    {
      title: "Date",
      key: "invoiceDate",
      sorter: (a, b) =>
        dayjs(
          getValue(
            a,
            "invoiceDate",
            "InvoiceDate"
          )
        ).valueOf() -
        dayjs(
          getValue(
            b,
            "invoiceDate",
            "InvoiceDate"
          )
        ).valueOf(),
      render: (_, record) => {
        const value = getValue(
          record,
          "invoiceDate",
          "InvoiceDate"
        );

        return value
          ? dayjs(value).format(
              "DD MMM YYYY"
            )
          : "-";
      },
    },
    {
      title: "Supplier",
      key: "supplierName",
      sorter: (a, b) =>
        String(
          getValue(
            a,
            "supplierName",
            "SupplierName"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "supplierName",
              "SupplierName"
            ) || ""
          )
        ),
      render: (_, record) =>
        getValue(
          record,
          "supplierName",
          "SupplierName"
        ) || "-",
    },
    {
      title: "Warehouse",
      key: "warehouseName",
      sorter: (a, b) =>
        String(
          getValue(
            a,
            "warehouseName",
            "WarehouseName"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "warehouseName",
              "WarehouseName"
            ) || ""
          )
        ),
      render: (_, record) =>
        getValue(
          record,
          "warehouseName",
          "WarehouseName"
        ) || "-",
    },
    {
      title: "Currency",
      key: "currencyCode",
      sorter: (a, b) =>
        String(
          getValue(
            a,
            "currencyCode",
            "CurrencyCode"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "currencyCode",
              "CurrencyCode"
            ) || ""
          )
        ),
      render: (_, record) =>
        getValue(
          record,
          "currencyCode",
          "CurrencyCode"
        ) || "-",
    },
    {
      title: "Sub Total",
      key: "subTotal",
      align: "right",
      sorter: (a, b) =>
        Number(
          getValue(
            a,
            "subTotal",
            "SubTotal"
          ) || 0
        ) -
        Number(
          getValue(
            b,
            "subTotal",
            "SubTotal"
          ) || 0
        ),
      render: (_, record) =>
        formatNumber(
          getValue(
            record,
            "subTotal",
            "SubTotal"
          )
        ),
    },
    {
      title: "Discount",
      key: "discountAmount",
      align: "right",
      sorter: (a, b) =>
        Number(
          getValue(
            a,
            "discountAmount",
            "DiscountAmount"
          ) || 0
        ) -
        Number(
          getValue(
            b,
            "discountAmount",
            "DiscountAmount"
          ) || 0
        ),
      render: (_, record) =>
        formatNumber(
          getValue(
            record,
            "discountAmount",
            "DiscountAmount"
          )
        ),
    },
    {
      title: "Tax",
      key: "taxAmount",
      align: "right",
      sorter: (a, b) =>
        Number(
          getValue(
            a,
            "taxAmount",
            "TaxAmount"
          ) || 0
        ) -
        Number(
          getValue(
            b,
            "taxAmount",
            "TaxAmount"
          ) || 0
        ),
      render: (_, record) =>
        formatNumber(
          getValue(
            record,
            "taxAmount",
            "TaxAmount"
          )
        ),
    },
    {
      title: "Total",
      key: "totalAmount",
      align: "right",
      sorter: (a, b) =>
        Number(
          getValue(
            a,
            "totalAmount",
            "TotalAmount"
          ) || 0
        ) -
        Number(
          getValue(
            b,
            "totalAmount",
            "TotalAmount"
          ) || 0
        ),
      render: (_, record) => (
        <strong>
          {formatNumber(
            getValue(
              record,
              "totalAmount",
              "TotalAmount"
            )
          )}
        </strong>
      ),
    },
    {
      title: "Status",
      key: "statusName",
      sorter: (a, b) =>
        String(
          getValue(
            a,
            "statusName",
            "StatusName"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "statusName",
              "StatusName"
            ) || ""
          )
        ),
      render: (_, record) => {
        const statusCode =
          String(
            getValue(
              record,
              "statusCode",
              "StatusCode"
            ) || ""
          ).toUpperCase();

        const statusName =
          getValue(
            record,
            "statusName",
            "StatusName"
          ) || "-";

        let color = "default";

        if (
          statusCode === "POSTED"
        ) {
          color = "green";
        } else if (
          statusCode === "DRAFT"
        ) {
          color = "orange";
        } else if (
          statusCode === "VOID"
        ) {
          color = "red";
        }

        return (
          <Tag color={color}>
            {statusName}
          </Tag>
        );
      },
    },
  ];

  return (
    <>
      {contextHolder}

      <style>
        {`
          .purchase-invoice-row {
            cursor: pointer;
            transition: all 0.2s ease;
          }

          .ant-table-tbody
            > tr.purchase-invoice-row:hover
            > td {
            background: #e6f4ff !important;
            color: #1677ff;
          }

          .ant-table-tbody
            > tr.purchase-invoice-row:hover
            > td
            strong {
            color: #0958d9;
          }

          .ant-table-tbody
            > tr.purchase-invoice-row:active
            > td {
            background: #bae0ff !important;
          }

          .purchase-summary-card {
            height: 100%;
          }
        `}
      </style>

      <div style={{ padding: 24 }}>
        <Row
          justify="space-between"
          align="middle"
          style={{ marginBottom: 20 }}
        >
          <Col>
            <Title
              level={2}
              style={{ margin: 0 }}
            >
              Purchase Invoices
            </Title>
          </Col>

          <Col>
            <Space orientation="horizontal">
              <Button
                icon={
                  <ReloadOutlined />
                }
                onClick={loadData}
                loading={loading}
              >
                Refresh
              </Button>

              <Button
                type="primary"
                icon={
                  <PlusOutlined />
                }
                onClick={onNewPurchase}
              >
                New Purchase
              </Button>
            </Space>
          </Col>
        </Row>

        <Card
          style={{
            marginBottom: 16,
            borderRadius: 10,
          }}
        >
          <Row
            gutter={[12, 12]}
            align="middle"
          >
            <Col>
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
                    value: "today",
                    label: "Today",
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
                    value: "all",
                    label: "All",
                  },
                  {
                    value: "custom",
                    label: "Custom",
                  },
                ]}
              />
            </Col>

            <Col>
              <Button
                icon={
                  <LeftOutlined />
                }
                onClick={
                  handlePreviousDay
                }
                disabled={
                  dateFilter === "all"
                }
              />
            </Col>

            <Col>
              <RangePicker
                value={dateRange}
                onChange={
                  handleRangeChange
                }
                format="DD MMM YYYY"
                allowClear
              />
            </Col>

            <Col>
              <Button
                icon={
                  <RightOutlined />
                }
                onClick={
                  handleNextDay
                }
                disabled={
                  dateFilter === "all"
                }
              />
            </Col>

            <Col flex="1">
              <Input.Search
                placeholder="Search invoice, supplier, warehouse..."
                allowClear
                value={searchText}
                onChange={(e) =>
                  setSearchText(
                    e.target.value
                  )
                }
              />
            </Col>
          </Row>
        </Card>

        <Row
          gutter={[16, 16]}
          style={{
            marginBottom: 16,
          }}
        >
          <Col xs={24} sm={8}>
            <Card className="purchase-summary-card">
              <Text type="secondary">
                Selected Period
              </Text>

              <div
                style={{
                  fontSize: 18,
                  fontWeight: 600,
                  marginTop: 6,
                }}
              >
                {dateRange?.[0]
                  ? dayjs(
                      dateRange[0]
                    ).format(
                      "DD MMM YYYY"
                    )
                  : "All Dates"}{" "}
                {dateRange?.[1]
                  ? `→ ${dayjs(
                      dateRange[1]
                    ).format(
                      "DD MMM YYYY"
                    )}`
                  : ""}
              </div>
            </Card>
          </Col>

          <Col xs={24} sm={8}>
            <Card className="purchase-summary-card">
              <Text type="secondary">
                Posted Invoices
              </Text>

              <div
                style={{
                  fontSize: 24,
                  fontWeight: 700,
                  marginTop: 6,
                }}
              >
                {postedInvoiceCount}
              </div>
            </Card>
          </Col>

          <Col xs={24} sm={8}>
            <Card className="purchase-summary-card">
              <Text type="secondary">
                Purchase Total (MMK)
              </Text>

              <div
                style={{
                  fontSize: 24,
                  fontWeight: 700,
                  marginTop: 6,
                }}
              >
                
                {formatNumber(
                  grandTotal
                )}
                {" "}Ks
              </div>
            </Card>
          </Col>
        </Row>

        <Card
          title="Purchase Invoice List"
          style={{
            borderRadius: 10,
            marginBottom: 16,
          }}
        >
          <Table
            rowKey={(record) =>
              getValue(
                record,
                "purchaseInvoiceId",
                "PurchaseInvoiceId"
              )
            }
            loading={loading}
            columns={columns}
            dataSource={filteredInvoices}
            rowClassName={() =>
              "purchase-invoice-row"
            }
            onRow={(record) => ({
              onDoubleClick: () =>
                onEditPurchase(
                  getValue(
                    record,
                    "purchaseInvoiceId",
                    "PurchaseInvoiceId"
                  )
                ),
              style: {
                cursor: "pointer",
              },
            })}
            pagination={{
              pageSize,
              showSizeChanger: true,
              pageSizeOptions: [
                10,
                20,
                50,
                100,
              ],
              showTotal: (
                total,
                range
              ) =>
                `${range[0]}-${range[1]} of ${total}`,
              onShowSizeChange: (
                _,
                size
              ) =>
                setPageSize(size),
            }}
          />
        </Card>

        <Card
          title="Daily Purchase Total"
          style={{
            borderRadius: 10,
          }}
        >
          <Table
            rowKey={(record) =>
              record.date.format(
                "YYYY-MM-DD"
              )
            }
            pagination={false}
            size="small"
            columns={[
              {
                title: "Date",
                dataIndex: "date",
                key: "date",
                render: (date) =>
                  date.format(
                    "DD MMM YYYY"
                  ),
              },
              {
                title: "Posted Invoices",
                dataIndex:
                  "invoiceCount",
                key: "invoiceCount",
                align: "center",
              },
              {
                title:
                  "Daily Purchase Total (MMK)",
                dataIndex:
                  "totalAmountMMK",
                key: "totalAmountMMK",
                align: "right",
                render: (value) => (
                  <strong>
                    Ks{" "}
                    {formatNumber(
                      value
                    )}
                  </strong>
                ),
              },
            ]}
            dataSource={dailySummary}
            locale={{
              emptyText:
                "No posted purchase invoices for the selected period.",
            }}
          />

          <Divider
            style={{
              margin: "16px 0",
            }}
          />

          <Row justify="end">
            <Space orientation="horizontal">
              <Text strong>
                Grand Total (MMK):
              </Text>

              <Text
                strong
                style={{
                  fontSize: 20,
                }}
              >
                Ks{" "}
                {formatNumber(
                  grandTotal
                )}
              </Text>
            </Space>
          </Row>
        </Card>
      </div>
    </>
  );
}
