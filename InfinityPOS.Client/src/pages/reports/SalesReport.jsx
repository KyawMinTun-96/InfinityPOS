import { useCallback, useEffect, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Row,
  Space,
  Spin,
  Statistic,
  Table,
  Tag,
  Typography,
} from "antd";
import { ReloadOutlined, SearchOutlined } from "@ant-design/icons";
import apiClient from "../../api/apiClient";

const { Title, Text } = Typography;

function getLocalDate(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function getFirstDayOfMonth() {
  const now = new Date();
  return getLocalDate(
    new Date(now.getFullYear(), now.getMonth(), 1)
  );
}

function formatDate(value) {
  if (!value) return "-";

  const date = String(value).slice(0, 10);
  const parts = date.split("-");

  if (parts.length !== 3) return date;

  return `${parts[2]}/${parts[1]}/${parts[0]}`;
}

function formatMoney(value) {
  return Number(value ?? 0).toLocaleString("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function currencyLabel(currencyId) {
  return `Currency #${currencyId ?? "-"}`;
}

export default function SalesReport() {
  const [fromDate, setFromDate] = useState(getFirstDayOfMonth);
  const [toDate, setToDate] = useState(() =>
    getLocalDate(new Date())
  );

  const [summary, setSummary] = useState([]);
  const [dailySales, setDailySales] = useState([]);
  const [payments, setPayments] = useState([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const loadReport = useCallback(async () => {
    if (!fromDate || !toDate) {
      setError("Please select both From Date and To Date.");
      return;
    }

    if (fromDate > toDate) {
      setError("From Date cannot be later than To Date.");
      return;
    }

    setLoading(true);
    setError("");

    try {
      const params = { fromDate, toDate };

      const [summaryResponse, dailyResponse, paymentResponse] =
        await Promise.all([
          apiClient.get("/SalesReports/summary", { params }),
          apiClient.get("/SalesReports/daily", { params }),
          apiClient.get("/SalesReports/payments", { params }),
        ]);

      setSummary(
        summaryResponse.data?.currencySummaries ?? []
      );

      setDailySales(
        dailyResponse.data?.dailySales ?? []
      );

      setPayments(
        paymentResponse.data?.paymentSummaries ?? []
      );
    } catch (err) {
      console.error("Failed to load Sales Report:", err);

      const status = err.response?.status;

      if (status === 401 || status === 403) {
        setError(
          "You are not authorized to access the Sales Report. Please check your login session and permissions."
        );
      } else if (!err.response) {
        setError(
          "Cannot connect to the API. Please check that the backend is running and VITE_API_BASE_URL is correct."
        );
      } else {
        setError(
          err.response?.data?.message ||
            `Failed to load Sales Report (HTTP ${status}).`
        );
      }

      setSummary([]);
      setDailySales([]);
      setPayments([]);
    } finally {
      setLoading(false);
    }
  }, [fromDate, toDate]);

  useEffect(() => {
    let cancelled = false;

    const run = async () => {
        if (cancelled) return;
        await loadReport();
    };

    const timer = setTimeout(run, 0);

    return () => {
        cancelled = true;
        clearTimeout(timer);
    };

  }, [loadReport]);

  const dailyColumns = [
    {
      title: "Date",
      dataIndex: "salesDate",
      key: "salesDate",
      render: formatDate,
    },
    {
      title: "Currency",
      dataIndex: "currencyId",
      key: "currencyId",
      render: currencyLabel,
    },
    {
      title: "Invoices",
      dataIndex: "invoiceCount",
      key: "invoiceCount",
      align: "right",
    },
    {
      title: "Subtotal",
      dataIndex: "subTotal",
      key: "subTotal",
      align: "right",
      render: formatMoney,
    },
    {
      title: "Discount",
      dataIndex: "discountAmount",
      key: "discountAmount",
      align: "right",
      render: formatMoney,
    },
    {
      title: "Tax",
      dataIndex: "taxAmount",
      key: "taxAmount",
      align: "right",
      render: formatMoney,
    },
    {
      title: "Net Sales",
      dataIndex: "netSales",
      key: "netSales",
      align: "right",
      render: (value) => (
        <Text strong>{formatMoney(value)}</Text>
      ),
    },
  ];

  const paymentColumns = [
    {
      title: "Payment Date",
      dataIndex: "paymentDate",
      key: "paymentDate",
      render: formatDate,
    },
    {
      title: "Payment Method",
      dataIndex: "paymentMethodName",
      key: "paymentMethodName",
      render: (value, record) => (
        <Space>
          <span>{value || "-"}</span>
          {record.paymentMethodCode && (
            <Tag>{record.paymentMethodCode}</Tag>
          )}
        </Space>
      ),
    },
    {
      title: "Currency",
      dataIndex: "currencyId",
      key: "currencyId",
      render: currencyLabel,
    },
    {
      title: "Payments",
      dataIndex: "paymentCount",
      key: "paymentCount",
      align: "right",
    },
    {
      title: "Received Amount",
      dataIndex: "receivedAmount",
      key: "receivedAmount",
      align: "right",
      render: (value) => (
        <Text strong>{formatMoney(value)}</Text>
      ),
    },
  ];

  return (
    <div>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          flexWrap: "wrap",
          gap: 12,
          marginBottom: 24,
        }}
      >
        <div>
          <Title level={2} style={{ margin: 0 }}>
            Sales Report
          </Title>
          <Text type="secondary">
            Posted sales invoices and payment collections
          </Text>
        </div>

        <Button
          icon={<ReloadOutlined />}
          onClick={loadReport}
          loading={loading}
        >
          Refresh
        </Button>
      </div>

      <Card
        title="Report Filters"
        style={{ marginBottom: 24 }}
      >
        <form
          onSubmit={(event) => {
            event.preventDefault();
            loadReport();
          }}
        >
          <Row gutter={[16, 16]} align="bottom">
            <Col xs={24} sm={10} md={8}>
              <label
                htmlFor="sales-report-from"
                style={{ display: "block", marginBottom: 8 }}
              >
                From Date
              </label>

              <input
                id="sales-report-from"
                type="date"
                value={fromDate}
                max={toDate || undefined}
                onChange={(event) =>
                  setFromDate(event.target.value)
                }
                required
                style={{
                  width: "100%",
                  height: 34,
                  padding: "4px 10px",
                  border: "1px solid #d9d9d9",
                  borderRadius: 6,
                  fontSize: 14,
                }}
              />
            </Col>

            <Col xs={24} sm={10} md={8}>
              <label
                htmlFor="sales-report-to"
                style={{ display: "block", marginBottom: 8 }}
              >
                To Date
              </label>

              <input
                id="sales-report-to"
                type="date"
                value={toDate}
                min={fromDate || undefined}
                onChange={(event) =>
                  setToDate(event.target.value)
                }
                required
                style={{
                  width: "100%",
                  height: 34,
                  padding: "4px 10px",
                  border: "1px solid #d9d9d9",
                  borderRadius: 6,
                  fontSize: 14,
                }}
              />
            </Col>

            <Col xs={24} sm={4} md={4}>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SearchOutlined />}
                loading={loading}
                block
              >
                Search
              </Button>
            </Col>
          </Row>
        </form>
      </Card>

      {error && (
        <Alert
          type="error"
          showIcon
          message="Sales Report Error"
          description={error}
          closable
          onClose={() => setError("")}
          style={{ marginBottom: 24 }}
        />
      )}

      <Spin spinning={loading}>
        <Title level={4}>Sales Summary</Title>

        {summary.length === 0 ? (
          <Card style={{ marginBottom: 24 }}>
            <Empty
              description={
                loading
                  ? "Loading sales summary..."
                  : "No posted sales found for this period."
              }
            />
          </Card>
        ) : (
          summary.map((item) => (
            <div key={item.currencyId} style={{ marginBottom: 24 }}>
              <Text
                type="secondary"
                style={{ display: "block", marginBottom: 12 }}
              >
                {currencyLabel(item.currencyId)}
              </Text>

              <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} xl={6}>
                  <Card>
                    <Statistic
                      title="Invoice Count"
                      value={item.invoiceCount ?? 0}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} xl={6}>
                  <Card>
                    <Statistic
                      title="Subtotal"
                      value={Number(item.subTotal ?? 0)}
                      precision={2}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} xl={6}>
                  <Card>
                    <Statistic
                      title="Discount"
                      value={Number(item.discountAmount ?? 0)}
                      precision={2}
                      valueStyle={{ color: "#cf1322" }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} xl={6}>
                  <Card>
                    <Statistic
                      title="Net Sales"
                      value={Number(item.netSales ?? 0)}
                      precision={2}
                      valueStyle={{ color: "#389e0d" }}
                    />
                  </Card>
                </Col>
              </Row>

              <Card
                size="small"
                style={{ marginTop: 12 }}
              >
                <Space wrap size="large">
                  <Text>
                    Tax: <Text strong>{formatMoney(item.taxAmount)}</Text>
                  </Text>

                  <Text>
                    Status: <Tag color="green">POSTED</Tag>
                  </Text>
                </Space>
              </Card>
            </div>
          ))
        )}

        <Card
          title="Daily Sales"
          style={{ marginBottom: 24 }}
        >
          <Table
            rowKey={(record) =>
              `${record.salesDate}-${record.currencyId}`
            }
            columns={dailyColumns}
            dataSource={dailySales}
            pagination={{ pageSize: 10, showSizeChanger: true }}
            scroll={{ x: 850 }}
            locale={{
              emptyText: "No daily sales for the selected period.",
            }}
            summary={(pageData) => {
              const totalInvoices = pageData.reduce(
                (sum, item) => sum + Number(item.invoiceCount ?? 0),
                0
              );

              return (
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={2}>
                    <Text strong>Page Total</Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={2} align="right">
                    <Text strong>{totalInvoices}</Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={3} align="right">
                    {formatMoney(
                      pageData.reduce(
                        (sum, item) => sum + Number(item.subTotal ?? 0),
                        0
                      )
                    )}
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={4} align="right">
                    {formatMoney(
                      pageData.reduce(
                        (sum, item) => sum + Number(item.discountAmount ?? 0),
                        0
                      )
                    )}
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={5} align="right">
                    {formatMoney(
                      pageData.reduce(
                        (sum, item) => sum + Number(item.taxAmount ?? 0),
                        0
                      )
                    )}
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={6} align="right">
                    <Text strong>
                      {formatMoney(
                        pageData.reduce(
                          (sum, item) => sum + Number(item.netSales ?? 0),
                          0
                        )
                      )}
                    </Text>
                  </Table.Summary.Cell>
                </Table.Summary.Row>
              );
            }}
          />
        </Card>

        <Card title="Payment Summary">
          <Table
            rowKey={(record) =>
              `${record.paymentDate}-${record.paymentMethodId}-${record.currencyId}`
            }
            columns={paymentColumns}
            dataSource={payments}
            pagination={{ pageSize: 10, showSizeChanger: true }}
            scroll={{ x: 700 }}
            locale={{
              emptyText: "No payment collections for the selected period.",
            }}
          />
        </Card>
      </Spin>
    </div>
  );
}