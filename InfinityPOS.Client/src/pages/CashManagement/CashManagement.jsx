
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import dayjs from "dayjs";
import {
  App,
  Button,
  Card,
  Col,
  DatePicker,
  Divider,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
} from "antd";
import {
  ArrowDownOutlined,
  ArrowUpOutlined,
  DollarOutlined,
  HistoryOutlined,
  LockOutlined,
  PlusOutlined,
  ReloadOutlined,
  UnlockOutlined,
} from "@ant-design/icons";

import {
  closeCashSession,
  createCashAdjustment,
  getCashLedger,
  getCashSessionHistory,
  getCashSessionByDate,
  openCashSession,
} from "../../api/cashApi";

const { RangePicker } = DatePicker;
const { Text, Title } = Typography;

/* =========================================================
   HELPERS
========================================================= */

function safeNumber(value, fallback = 0) {
  if (value === null || value === undefined || value === "") {
    return fallback;
  }

  const numberValue = Number(value);

  return Number.isFinite(numberValue) ? numberValue : fallback;
}

function formatNumber(value) {
  return safeNumber(value).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function formatDate(value) {
  if (!value) return "-";

  const date = dayjs(value);

  return date.isValid() ? date.format("DD/MM/YYYY HH:mm") : "-";
}

function formatBusinessDate(value) {
  if (!value) return "-";

  const date = dayjs(value);

  return date.isValid() ? date.format("DD/MM/YYYY") : "-";
}

/* =========================================================
   COMPONENT
========================================================= */

export default function CashManagement() {
  const { message } = App.useApp();

  const [loading, setLoading] = useState(false);
  const [session, setSession] = useState(null);
  const [ledger, setLedger] = useState([]);
  const [history, setHistory] = useState([]);

  // Selected Business Date is the source of truth.
  const [selectedDate, setSelectedDate] = useState(() => dayjs());

  const [historyRange, setHistoryRange] = useState(() => [
    dayjs().subtract(30, "day"),
    dayjs(),
  ]);

  const [adjustmentType, setAdjustmentType] = useState("CASH_IN");

  const [openModal, setOpenModal] = useState(false);
  const [closeModal, setCloseModal] = useState(false);
  const [adjustmentModal, setAdjustmentModal] = useState(false);

  const [openForm] = Form.useForm();
  const [closeForm] = Form.useForm();
  const [adjustmentForm] = Form.useForm();

  // Ignore stale responses when the date changes quickly.
  const sessionRequestId = useRef(0);

  /* =========================================================
     LOAD SESSION AND LEDGER
  ========================================================= */

  const loadSessionByDate = useCallback(
    async (date) => {
      if (!date || !dayjs(date).isValid()) {
        return;
      }

      const requestId = ++sessionRequestId.current;
      const businessDate = dayjs(date).format("YYYY-MM-DD");

      try {
        const data = await getCashSessionByDate(businessDate);

        if (requestId !== sessionRequestId.current) {
          return;
        }

        setSession(data || null);

        if (data?.cashSessionId) {
          const ledgerData = await getCashLedger(data.cashSessionId);

          if (requestId !== sessionRequestId.current) {
            return;
          }

          setLedger(Array.isArray(ledgerData) ? ledgerData : []);
        } else {
          setLedger([]);
        }
      } catch (error) {
        if (requestId !== sessionRequestId.current) {
          return;
        }

        setSession(null);
        setLedger([]);

        if (error?.response?.status === 404) {
          message.info("No cash session found for this date.");
        } else {
          console.error("Cash Session Date Error:", error);

          message.error(
            error?.response?.data?.message ||
              error?.message ||
              "Failed to load cash session."
          );
        }
      } finally {
        if (requestId === sessionRequestId.current) {
          setLoading(false);
        }
      }
    },
    [message]
  );

  /* =========================================================
     LOAD HISTORY
  ========================================================= */

  /* =========================================================
     LOAD HISTORY
  ========================================================= */

  const loadHistory = useCallback(async () => {
    try {
      const from = historyRange?.[0]?.format("YYYY-MM-DD");
      const to = historyRange?.[1]?.format("YYYY-MM-DD");

      const data = await getCashSessionHistory(from, to);

      setHistory(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Cash History Error:", error);

      message.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to load cash history."
      );
    }
  }, [historyRange, message]);

  /* =========================================================
     INITIAL LOAD / DATE CHANGE

     State updates happen after the asynchronous API response.
     This avoids synchronous state updates inside the effect.
  ========================================================= */

  useEffect(() => {
    let cancelled = false;
    const businessDate = selectedDate.format("YYYY-MM-DD");
    const requestId = ++sessionRequestId.current;

    const fetchSelectedSession = async () => {
      try {
        const data = await getCashSessionByDate(businessDate);

        if (cancelled || requestId !== sessionRequestId.current) {
          return;
        }

        setSession(data || null);

        if (data?.cashSessionId) {
          const ledgerData = await getCashLedger(data.cashSessionId);

          if (cancelled || requestId !== sessionRequestId.current) {
            return;
          }

          setLedger(Array.isArray(ledgerData) ? ledgerData : []);
        } else {
          setLedger([]);
        }
      } catch (error) {
        if (cancelled || requestId !== sessionRequestId.current) {
          return;
        }

        setSession(null);
        setLedger([]);

        if (error?.response?.status === 404) {
          message.info("No cash session found for this date.");
        } else {
          console.error("Cash Session Date Error:", error);

          message.error(
            error?.response?.data?.message ||
              error?.message ||
              "Failed to load cash session."
          );
        }
      } finally {
        if (!cancelled && requestId === sessionRequestId.current) {
          setLoading(false);
        }
      }
    };

    void fetchSelectedSession();

    return () => {
      cancelled = true;
      sessionRequestId.current += 1;
    };
  }, [selectedDate, message]);

  useEffect(() => {
  }, [loadHistory]);

  /* =========================================================
     DATE CHANGE
  ========================================================= */

  const handleDateChange = (value) => {
    if (!value) return;

    // Invalidate any pending request for the previous date.
    sessionRequestId.current += 1;

    // Keep the selected date as the source of truth.
    setSelectedDate(value);
  };

  /* =========================================================
     OPEN CASH
  ========================================================= */

  const handleOpenCash = async () => {
    try {
      const values = await openForm.validateFields();

      setLoading(true);

      const businessDate = values.businessDate
        ? values.businessDate.format("YYYY-MM-DD")
        : selectedDate.format("YYYY-MM-DD");

      const data = await openCashSession({
        businessDate,
        openingCash: safeNumber(values.openingCash),
        openedByUserId: values.openedByUserId || null,
        notes: values.notes || null,
      });

      setOpenModal(false);
      openForm.resetFields();

      message.success("Cash session opened successfully.");

      const openedDate = data?.businessDate
        ? dayjs(data.businessDate)
        : dayjs(businessDate);

      setSelectedDate(openedDate);

      // Immediately load the newly opened date.
      await loadSessionByDate(openedDate);
      await loadHistory();
    } catch (error) {
      if (error?.errorFields) return;

      console.error("Open Cash Error:", error);

      message.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to open cash session."
      );
    } finally {
      setLoading(false);
    }
  };

  /* =========================================================
     CLOSE CASH
  ========================================================= */

  const handleCloseCash = async () => {
    if (!session?.cashSessionId) return;

    try {
      const values = await closeForm.validateFields();

      setLoading(true);

      await closeCashSession(session.cashSessionId, {
        actualClosingCash: safeNumber(values.actualClosingCash),
        closedByUserId: values.closedByUserId || null,
        notes: values.notes || null,
      });

      setCloseModal(false);
      closeForm.resetFields();

      message.success("Cash session closed successfully.");

      await loadSessionByDate(selectedDate);
      await loadHistory();
    } catch (error) {
      if (error?.errorFields) return;

      console.error("Close Cash Error:", error);

      message.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to close cash session."
      );
    } finally {
      setLoading(false);
    }
  };

  /* =========================================================
     CASH ADJUSTMENT
  ========================================================= */

  const handleAdjustment = async () => {
    if (!session?.cashSessionId) return;

    try {
      const values = await adjustmentForm.validateFields();

      setLoading(true);

      await createCashAdjustment(session.cashSessionId, {
        transactionType: adjustmentType,
        amount: safeNumber(values.amount),
        transactionDate: dayjs().format("YYYY-MM-DDTHH:mm:ss"),
        referenceNumber: values.referenceNumber || null,
        description: values.description || null,
        createdByUserId: values.createdByUserId || null,
      });

      setAdjustmentModal(false);
      adjustmentForm.resetFields();

      message.success(
        adjustmentType === "CASH_IN"
          ? "Cash In added successfully."
          : "Cash Out added successfully."
      );

      await loadSessionByDate(selectedDate);
      await loadHistory();
    } catch (error) {
      if (error?.errorFields) return;

      console.error("Cash Adjustment Error:", error);

      message.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to add cash adjustment."
      );
    } finally {
      setLoading(false);
    }
  };

  /* =========================================================
     REFRESH
  ========================================================= */

  const handleRefresh = async () => {
    setLoading(true);

    await Promise.all([
      loadSessionByDate(selectedDate),
      loadHistory(),
    ]);
  };

  /* =========================================================
     STATISTICS
  ========================================================= */

  const openingCash = safeNumber(session?.openingCash);
  const systemCash = safeNumber(session?.systemCash);

  const actualClosingCash =
    session?.closingCash !== null &&
    session?.closingCash !== undefined
      ? safeNumber(session.closingCash)
      : null;

  const difference =
    session?.difference !== null &&
    session?.difference !== undefined
      ? safeNumber(session.difference)
      : null;

  const cashSales = safeNumber(session?.cashSales);
  const cashPurchases = safeNumber(session?.cashPurchases);
  const cashExpenses = safeNumber(session?.cashExpenses);
  const manualCashIn = safeNumber(session?.manualCashIn);
  const manualCashOut = safeNumber(session?.manualCashOut);

  /* =========================================================
     LEDGER COLUMNS
  ========================================================= */

  const ledgerColumns = useMemo(
    () => [
      {
        title: "#",
        key: "index",
        width: 50,
        align: "center",
        render: (_, __, index) => index + 1,
      },
      {
        title: "DATE",
        dataIndex: "transactionDate",
        key: "transactionDate",
        width: 150,
        render: (value) => formatDate(value),
      },
      {
        title: "TYPE",
        dataIndex: "transactionType",
        key: "transactionType",
        width: 110,
        render: (value) => {
          let color = "default";

          if (
            value === "SALE" ||
            value === "CASH_IN" ||
            value === "OPENING"
          ) {
            color = "success";
          }

          if (
            value === "PURCHASE" ||
            value === "EXPENSE" ||
            value === "CASH_OUT"
          ) {
            color = "error";
          }

          return <Tag color={color}>{value}</Tag>;
        },
      },
      {
        title: "VOUCHER",
        dataIndex: "voucherNumber",
        key: "voucherNumber",
        width: 130,
        render: (value) => value || "-",
      },
      {
        title: "DESCRIPTION",
        dataIndex: "description",
        key: "description",
        width: 240,
        ellipsis: true,
      },
      {
        title: "CASH IN",
        dataIndex: "cashIn",
        key: "cashIn",
        width: 110,
        align: "right",
        render: (value) =>
          safeNumber(value) > 0 ? (
            <Text strong style={{ color: "#3f8600" }}>
              +{formatNumber(value)}
            </Text>
          ) : (
            "-"
          ),
      },
      {
        title: "CASH OUT",
        dataIndex: "cashOut",
        key: "cashOut",
        width: 110,
        align: "right",
        render: (value) =>
          safeNumber(value) > 0 ? (
            <Text strong style={{ color: "#cf1322" }}>
              −{formatNumber(value)}
            </Text>
          ) : (
            "-"
          ),
      },
      {
        title: "BALANCE",
        dataIndex: "balance",
        key: "balance",
        width: 120,
        align: "right",
        render: (value) => (
          <Text strong>{formatNumber(value)}</Text>
        ),
      },
    ],
    []
  );

  /* =========================================================
     HISTORY COLUMNS
  ========================================================= */

  const historyColumns = useMemo(
    () => [
      {
        title: "#",
        key: "index",
        width: 50,
        align: "center",
        render: (_, __, index) => index + 1,
      },
      {
        title: "DATE",
        dataIndex: "businessDate",
        key: "businessDate",
        width: 120,
        render: (value) => formatBusinessDate(value),
      },
      {
        title: "STATUS",
        dataIndex: "status",
        key: "status",
        width: 100,
        render: (value) => (
          <Tag color={value === "OPEN" ? "green" : "default"}>
            {value}
          </Tag>
        ),
      },
      {
        title: "OPENING",
        dataIndex: "openingCash",
        key: "openingCash",
        width: 120,
        align: "right",
        render: (value) => formatNumber(value),
      },
      {
        title: "SALES",
        dataIndex: "cashSales",
        key: "cashSales",
        width: 120,
        align: "right",
        render: (value) => (
          <Text style={{ color: "#3f8600" }}>
            +{formatNumber(value)}
          </Text>
        ),
      },
      {
        title: "PURCHASE",
        dataIndex: "cashPurchases",
        key: "cashPurchases",
        width: 120,
        align: "right",
        render: (value) => (
          <Text style={{ color: "#cf1322" }}>
            −{formatNumber(value)}
          </Text>
        ),
      },
      {
        title: "EXPENSE",
        dataIndex: "cashExpenses",
        key: "cashExpenses",
        width: 120,
        align: "right",
        render: (value) => (
          <Text style={{ color: "#cf1322" }}>
            −{formatNumber(value)}
          </Text>
        ),
      },
      {
        title: "SYSTEM CASH",
        dataIndex: "systemCash",
        key: "systemCash",
        width: 130,
        align: "right",
        render: (value) => (
          <Text strong>{formatNumber(value)}</Text>
        ),
      },
      {
        title: "ACTUAL",
        dataIndex: "closingCash",
        key: "closingCash",
        width: 120,
        align: "right",
        render: (value) =>
          value === null || value === undefined
            ? "-"
            : formatNumber(value),
      },
      {
        title: "DIFFERENCE",
        dataIndex: "difference",
        key: "difference",
        width: 120,
        align: "right",
        render: (value) => {
          if (value === null || value === undefined) return "-";

          const number = safeNumber(value);

          return (
            <Text
              strong
              type={
                number === 0
                  ? undefined
                  : number > 0
                    ? "success"
                    : "danger"
              }
            >
              {number > 0 ? "+" : ""}
              {formatNumber(number)}
            </Text>
          );
        },
      },
    ],
    []
  );

  /* =========================================================
     RENDER
  ========================================================= */

  return (
    <div>
      {/* HEADER */}

      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: 16,
          gap: 12,
          flexWrap: "wrap",
        }}
      >
        <div>
          <Title level={3} style={{ margin: 0 }}>
            <DollarOutlined /> Cash Management
          </Title>

          <Text type="secondary">
            Daily cash drawer management and cash ledger
          </Text>
        </div>

        <Space wrap>
          <DatePicker
            value={selectedDate}
            allowClear={false}
            onChange={handleDateChange}
          />

          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefresh}
            loading={loading}
          >
            Refresh
          </Button>

          {(!session || session.status !== "OPEN") && (
            <Button
              type="primary"
              icon={<UnlockOutlined />}
              onClick={() => {
                openForm.setFieldsValue({
                  businessDate: selectedDate,
                  openingCash: 0,
                });
                setOpenModal(true);
              }}
            >
              Open Cash
            </Button>
          )}

          {session?.status === "OPEN" && (
            <>
              <Button
                icon={<PlusOutlined />}
                onClick={() => {
                  setAdjustmentType("CASH_IN");
                  setAdjustmentModal(true);
                }}
              >
                Cash In / Out
              </Button>

              <Button
                danger
                icon={<LockOutlined />}
                onClick={() => setCloseModal(true)}
              >
                Close Cash
              </Button>
            </>
          )}
        </Space>
      </div>

      {/* SESSION STATUS */}

      <Card size="small" style={{ marginBottom: 16 }}>
        <Row gutter={[16, 8]} align="middle">
          <Col>
            <Text type="secondary">Selected Business Date</Text>
            <div>
              <Text strong>
                {formatBusinessDate(selectedDate)}
              </Text>
            </div>
          </Col>

          <Col>
            <Divider orientation="vertical" style={{ height: 40 }} />
          </Col>

          <Col>
            <Text type="secondary">Session Business Date</Text>
            <div>
              <Text strong>
                {formatBusinessDate(session?.businessDate)}
              </Text>
            </div>
          </Col>

          <Col>
            <Divider orientation="vertical" style={{ height: 40 }} />
          </Col>

          <Col>
            <Text type="secondary">Session Status</Text>
            <div>
              {session ? (
                <Tag color={session.status === "OPEN" ? "success" : "default"}>
                  {session.status}
                </Tag>
              ) : (
                <Tag>NOT OPEN</Tag>
              )}
            </div>
          </Col>

          {session?.cashSessionId && (
            <>
              <Col>
                <Divider orientation="vertical" style={{ height: 40 }} />
              </Col>

              <Col>
                <Text type="secondary">Session ID</Text>
                <div>
                  <Text strong>#{session.cashSessionId}</Text>
                </div>
              </Col>
            </>
          )}
        </Row>
      </Card>

      {/* SUMMARY CARDS */}

      <Row gutter={[12, 12]} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic title="OPENING CASH" value={openingCash} precision={2} />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="CASH SALES"
              value={cashSales}
              precision={2}
              prefix="+"
              styles={{ content: { color: "#3f8600" } }}
            />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="CASH PURCHASE"
              value={cashPurchases}
              precision={2}
              prefix="−"
              styles={{ content: { color: "#cf1322" } }}
            />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="CASH EXPENSE"
              value={cashExpenses}
              precision={2}
              prefix="−"
              styles={{ content: { color: "#cf1322" } }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[12, 12]} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="MANUAL CASH IN"
              value={manualCashIn}
              precision={2}
              prefix="+"
              styles={{ content: { color: "#3f8600" } }}
            />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="MANUAL CASH OUT"
              value={manualCashOut}
              precision={2}
              prefix="−"
              styles={{ content: { color: "#cf1322" } }}
            />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="SYSTEM CASH"
              value={systemCash}
              precision={2}
              styles={{
                content: {
                  fontSize: 22,
                  fontWeight: 700,
                },
              }}
            />
          </Card>
        </Col>

        <Col xs={12} sm={12} md={6}>
          <Card size="small">
            <Statistic
              title="ACTUAL CLOSING"
              value={actualClosingCash ?? 0}
              precision={2}
              styles={{
                content: {
                  fontSize: 22,
                  fontWeight: 700,
                },
              }}
            />

            {actualClosingCash === null && (
              <Text type="secondary" style={{ fontSize: 11 }}>
                Not closed yet
              </Text>
            )}
          </Card>
        </Col>
      </Row>

      {/* CASH DIFFERENCE */}

      {difference !== null && (
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row align="middle" justify="space-between">
            <Col>
              <Text type="secondary">CASH DIFFERENCE</Text>
            </Col>

            <Col>
              <Text
                strong
                style={{
                  fontSize: 24,
                  color:
                    difference === 0
                      ? "#3f8600"
                      : difference > 0
                        ? "#1677ff"
                        : "#cf1322",
                }}
              >
                {difference > 0 ? "+" : ""}
                {formatNumber(difference)}
              </Text>
            </Col>
          </Row>
        </Card>
      )}

      {/* DAILY CASH LEDGER */}

      <Card
        size="small"
        title={
          <Space>
            <HistoryOutlined />
            <span>Daily Cash Ledger</span>
          </Space>
        }
        style={{ marginBottom: 16 }}
      >
        {!session ? (
          <Empty description="No cash session for this date" />
        ) : ledger.length === 0 ? (
          <Empty description="No cash transactions found" />
        ) : (
          <Table
            size="small"
            bordered
            loading={loading}
            columns={ledgerColumns}
            dataSource={ledger}
            rowKey={(record) =>
              `${record.source ?? record.transactionType ?? "entry"}-${record.referenceId ?? record.voucherNumber ?? record.transactionDate}`
            }
            scroll={{ x: 950 }}
            pagination={{
              defaultPageSize: 100,
              pageSizeOptions: [100, 200, 500],
              showSizeChanger: true,
              showTotal: (total) => `Total ${total} entries`,
            }}
          />
        )}
      </Card>

      {/* SESSION HISTORY */}

      <Card
        size="small"
        title={
          <Space>
            <HistoryOutlined />
            <span>Cash Session History</span>
          </Space>
        }
        extra={
          <RangePicker
            value={historyRange}
            onChange={(values) => {
              if (values && values.length === 2 && values[0] && values[1]) {
                setHistoryRange(values);
              }
            }}
          />
        }
      >
        {history.length === 0 ? (
          <Empty description="No cash session history" />
        ) : (
          <Table
            size="small"
            bordered
            columns={historyColumns}
            dataSource={history}
            rowKey="cashSessionId"
            scroll={{ x: 1250 }}
            pagination={{
              defaultPageSize: 30,
              pageSizeOptions: [30, 50, 100],
              showSizeChanger: true,
              showTotal: (total) => `Total ${total} sessions`,
            }}
          />
        )}
      </Card>

      {/* OPEN CASH MODAL */}

      <Modal
        title={
          <Space>
            <UnlockOutlined />
            <span>Open Cash Session</span>
          </Space>
        }
        open={openModal}
        onCancel={() => {
          setOpenModal(false);
          openForm.resetFields();
        }}
        onOk={handleOpenCash}
        okText="Open Cash"
        confirmLoading={loading}
        destroyOnHidden
      >
        <Form
          form={openForm}
          layout="vertical"
          initialValues={{
            businessDate: selectedDate,
            openingCash: 0,
          }}
        >
          <Form.Item
            label="Business Date"
            name="businessDate"
            rules={[
              {
                required: true,
                message: "Please select business date.",
              },
            ]}
          >
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>

          <Form.Item
            label="Opening Cash"
            name="openingCash"
            rules={[
              {
                required: true,
                message: "Please enter opening cash.",
              },
            ]}
          >
            <InputNumber
              min={0}
              precision={2}
              style={{ width: "100%" }}
              placeholder="0.00"
            />
          </Form.Item>

          <Form.Item label="Opened By User ID" name="openedByUserId">
            <InputNumber
              min={1}
              style={{ width: "100%" }}
              placeholder="Optional"
            />
          </Form.Item>

          <Form.Item label="Notes" name="notes">
            <Input.TextArea rows={3} placeholder="Optional notes" />
          </Form.Item>
        </Form>
      </Modal>

      {/* CLOSE CASH MODAL */}

      <Modal
        title={
          <Space>
            <LockOutlined />
            <span>Close Cash Session</span>
          </Space>
        }
        open={closeModal}
        onCancel={() => {
          setCloseModal(false);
          closeForm.resetFields();
        }}
        onOk={handleCloseCash}
        okText="Close Cash"
        okButtonProps={{ danger: true }}
        confirmLoading={loading}
        destroyOnHidden
      >
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row gutter={[12, 12]}>
            <Col span={12}>
              <Statistic title="System Cash" value={systemCash} precision={2} />
            </Col>

            <Col span={12}>
              <Statistic title="Expected Cash" value={systemCash} precision={2} />
            </Col>
          </Row>
        </Card>

        <Form form={closeForm} layout="vertical">
          <Form.Item
            label="Actual Closing Cash"
            name="actualClosingCash"
            rules={[
              {
                required: true,
                message: "Please enter actual closing cash.",
              },
            ]}
          >
            <InputNumber
              min={0}
              precision={2}
              style={{ width: "100%" }}
              placeholder="Counted cash"
            />
          </Form.Item>

          <Form.Item label="Closed By User ID" name="closedByUserId">
            <InputNumber
              min={1}
              style={{ width: "100%" }}
              placeholder="Optional"
            />
          </Form.Item>

          <Form.Item label="Notes" name="notes">
            <Input.TextArea rows={3} placeholder="Optional notes" />
          </Form.Item>
        </Form>
      </Modal>

      {/* CASH IN / OUT MODAL */}

      <Modal
        title={
          <Space>
            {adjustmentType === "CASH_IN" ? (
              <ArrowUpOutlined />
            ) : (
              <ArrowDownOutlined />
            )}

            <span>
              {adjustmentType === "CASH_IN"
                ? "Manual Cash In"
                : "Manual Cash Out"}
            </span>
          </Space>
        }
        open={adjustmentModal}
        onCancel={() => {
          setAdjustmentModal(false);
          adjustmentForm.resetFields();
        }}
        onOk={handleAdjustment}
        okText="Save"
        confirmLoading={loading}
        destroyOnHidden
      >
        <Form form={adjustmentForm} layout="vertical">
          <Form.Item label="Transaction Type" required>
            <Select
              value={adjustmentType}
              onChange={setAdjustmentType}
              options={[
                { value: "CASH_IN", label: "Cash In" },
                { value: "CASH_OUT", label: "Cash Out" },
              ]}
            />
          </Form.Item>

          <Form.Item
            label="Amount"
            name="amount"
            rules={[
              {
                required: true,
                message: "Please enter amount.",
              },
            ]}
          >
            <InputNumber
              min={0.01}
              precision={2}
              style={{ width: "100%" }}
              placeholder="0.00"
            />
          </Form.Item>

          <Form.Item label="Reference Number" name="referenceNumber">
            <Input placeholder="Optional" />
          </Form.Item>

          <Form.Item label="Description" name="description">
            <Input.TextArea rows={3} placeholder="Reason / description" />
          </Form.Item>

          <Form.Item label="Created By User ID" name="createdByUserId">
            <InputNumber
              min={1}
              style={{ width: "100%" }}
              placeholder="Optional"
            />
          </Form.Item>
        </Form>
      </Modal>

      {/* CSS */}

      <style>
        {`
          .ant-table-small
            .ant-table-thead > tr > th {
            padding: 7px 8px !important;
            font-size: 12px;
            white-space: nowrap;
          }

          .ant-table-small
            .ant-table-tbody > tr > td {
            padding: 6px 8px !important;
            font-size: 12px;
          }

          .ant-table-tbody > tr:hover > td {
            background: #fafafa !important;
          }
        `}
      </style>
    </div>
  );
}
