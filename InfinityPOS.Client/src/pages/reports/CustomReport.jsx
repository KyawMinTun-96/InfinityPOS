
import { useState } from "react";
import {
  Alert,
  Button,
  Card,
  Checkbox,
  Col,
  DatePicker,
  Empty,
  message,
  Row,
  Select,
  Space,
  Spin,
  Table,
  Typography,
} from "antd";
import {
  ReloadOutlined,
  PrinterOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import apiClient from "../../api/apiClient";

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const REPORT_TYPES = {
  summary: "Sales Summary",
  daily: "Daily Sales - Invoice Details",
  items: "Sales Item Report",
  payments: "Payment Summary",
};

const REPORT_COLUMNS = {
  summary: [
    { title: "Currency ID", dataIndex: "currencyId", key: "currencyId" },
    { title: "Invoice Count", dataIndex: "invoiceCount", key: "invoiceCount" },
    { title: "Subtotal", dataIndex: "subTotal", key: "subTotal", numeric: true },
    { title: "Discount", dataIndex: "discountAmount", key: "discountAmount", numeric: true },
    { title: "Tax", dataIndex: "taxAmount", key: "taxAmount", numeric: true },
    { title: "Net Sales", dataIndex: "netSales", key: "netSales", numeric: true },
  ],

  daily: [
    { title: "SKU", dataIndex: "sku", key: "sku" },
    { title: "Product", dataIndex: "productName", key: "productName" },
    { title: "Qty", dataIndex: "quantity", key: "quantity", numeric: true },
    { title: "Unit Price", dataIndex: "unitPrice", key: "unitPrice", numeric: true },
    { title: "Discount", dataIndex: "discountAmount", key: "discountAmount", numeric: true },
    { title: "Tax", dataIndex: "taxAmount", key: "taxAmount", numeric: true },
    { title: "Line Total", dataIndex: "lineTotal", key: "lineTotal", numeric: true },
  ],

  items: [
    {
      title: "Sale Date",
      dataIndex: "invoiceDate",
      key: "invoiceDate",
      width: 165,
    },
    {
      title: "Voucher No.",
      dataIndex: "invoiceNumber",
      key: "invoiceNumber",
      width: 150,
    },
    { title: "SKU", dataIndex: "sku", key: "sku" },
    { title: "Product Name", dataIndex: "productName", key: "productName" },
    { title: "Qty", dataIndex: "quantity", key: "quantity", numeric: true },
    { title: "Unit Price", dataIndex: "unitPrice", key: "unitPrice", numeric: true },
    { title: "Discount", dataIndex: "discountAmount", key: "discountAmount", numeric: true },
    { title: "Tax", dataIndex: "taxAmount", key: "taxAmount", numeric: true },
    { title: "Net Amount", dataIndex: "lineTotal", key: "lineTotal", numeric: true },
    { title: "Currency ID", dataIndex: "currencyId", key: "currencyId" },
  ],

  payments: [
    { title: "Payment Date", dataIndex: "paymentDate", key: "paymentDate" },
    { title: "Payment Method", dataIndex: "paymentMethodName", key: "paymentMethodName" },
    { title: "Payment Code", dataIndex: "paymentMethodCode", key: "paymentMethodCode" },
    { title: "Currency ID", dataIndex: "currencyId", key: "currencyId" },
    { title: "Payment Count", dataIndex: "paymentCount", key: "paymentCount" },
    { title: "Received Amount", dataIndex: "receivedAmount", key: "receivedAmount", numeric: true },
  ],
};

const ENDPOINTS = {
  summary: "/SalesReports/summary",
  daily: "/SalesReports/daily",
  items: "/SalesReports/items",
  payments: "/SalesReports/payments",
};

const RESPONSE_KEYS = {
  summary: "currencySummaries",
  daily: "invoiceDetails",
  items: "salesItems",
  payments: "paymentSummaries",
};

const formatNumber = (value) =>
  Number(value ?? 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

const formatDate = (value, includeTime = false) => {
  if (!value) return "-";

  const parsed = dayjs(value);
  if (!parsed.isValid()) return "-";

  return parsed.format(
    includeTime ? "YYYY-MM-DD HH:mm" : "YYYY-MM-DD"
  );
};

export default function CustomReport() {
  const [reportType, setReportType] = useState("daily");

  const [dateRange, setDateRange] = useState([
    dayjs().startOf("month"),
    dayjs(),
  ]);

  const [selectedColumns, setSelectedColumns] = useState(
    REPORT_COLUMNS.daily.map((column) => column.key)
  );

  const [data, setData] = useState([]);
  const [dailyTotals, setDailyTotals] = useState([]);
  const [itemTotals, setItemTotals] = useState([]);
  const [loading, setLoading] = useState(false);
  const [hasPreview, setHasPreview] = useState(false);
  const [error, setError] = useState("");

  const changeReportType = (value) => {
    setReportType(value);
    setSelectedColumns(
      REPORT_COLUMNS[value].map((column) => column.key)
    );
    setData([]);
    setDailyTotals([]);
    setItemTotals([]);
    setHasPreview(false);
    setError("");
  };

  const loadReport = async () => {
    if (!dateRange?.[0] || !dateRange?.[1]) {
      message.warning("Please select a date range.");
      return;
    }

    if (dateRange[0].isAfter(dateRange[1], "day")) {
      message.warning("From Date cannot be later than To Date.");
      return;
    }

    setLoading(true);
    setError("");
    setHasPreview(false);

    try {
      const response = await apiClient.get(ENDPOINTS[reportType], {
        params: {
          fromDate: dateRange[0].format("YYYY-MM-DD"),
          toDate: dateRange[1].format("YYYY-MM-DD"),
        },
      });

      const rows = response.data?.[RESPONSE_KEYS[reportType]];

      if (!Array.isArray(rows)) {
        throw new Error(
          `The ${REPORT_TYPES[reportType]} API did not return the expected data. Check the backend endpoint.`
        );
      }

      setData(
        rows.map((row, index) => ({
          ...row,
          key:
            row.salesInvoiceItemId ??
            row.salesInvoiceId ??
            `${reportType}-${index}`,
        }))
      );

      setDailyTotals(
        reportType === "daily"
          ? response.data?.dailyTotals ?? []
          : []
      );

      setItemTotals(
        reportType === "items"
          ? response.data?.totals ?? []
          : []
      );

      setHasPreview(true);
    } catch (err) {
      const detail =
        err.response?.data?.message ||
        err.message ||
        "Unable to load report.";

      setError(detail);
      setData([]);
      setDailyTotals([]);
      setItemTotals([]);
      setHasPreview(false);
      message.error(detail);
    } finally {
      setLoading(false);
    }
  };

  const columns = REPORT_COLUMNS[reportType]
    .filter((column) => selectedColumns.includes(column.key))
    .map((column) => ({
      ...column,
      render: (value) => {
        if (column.numeric) return formatNumber(value);

        if (column.key === "invoiceDate") {
          return formatDate(value, true);
        }

        if (
          column.key === "paymentDate" ||
          column.key === "salesDate"
        ) {
          return formatDate(value);
        }

        return value ?? "-";
      },
    }));

  const printReport = () => {
    if (!hasPreview) {
      message.warning("Please preview the report first.");
      return;
    }

    if (selectedColumns.length === 0) {
      message.warning("Please select at least one column.");
      return;
    }

    window.print();
  };

  const renderInvoiceDetails = () => {
    if (data.length === 0) {
      return (
        <Empty description="No posted invoices found for this date range." />
      );
    }

    return (
      <>
        {data.map((invoice) => {
          const products = (invoice.products ?? []).map((product, index) => ({
            ...product,
            key:
              product.salesInvoiceItemId ??
              `${invoice.salesInvoiceId}-${index}`,
          }));

          return (
            <Card
              key={invoice.salesInvoiceId}
              className="invoice-detail-card"
              style={{ marginBottom: 16 }}
              title={
                <div>
                  <Text strong>Invoice No: {invoice.invoiceNumber}</Text>
                  <br />
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    Date: {formatDate(invoice.invoiceDate, true)}
                    {" | "}
                    Currency ID: {invoice.currencyId}
                  </Text>
                </div>
              }
            >
              <Table
                size="small"
                bordered
                pagination={false}
                rowKey="key"
                columns={columns}
                dataSource={products}
                scroll={{ x: "max-content" }}
                locale={{ emptyText: "No product lines in this invoice." }}
              />

              <Row justify="end" style={{ marginTop: 16 }}>
                <Col xs={24} sm={14} md={10} lg={8}>
                  <Space direction="vertical" style={{ width: "100%" }} size={8}>
                    <Row justify="space-between">
                      <Text>Subtotal</Text>
                      <Text>{formatNumber(invoice.subTotal)}</Text>
                    </Row>
                    <Row justify="space-between">
                      <Text>Discount</Text>
                      <Text>{formatNumber(invoice.discountAmount)}</Text>
                    </Row>
                    <Row justify="space-between">
                      <Text>Tax</Text>
                      <Text>{formatNumber(invoice.taxAmount)}</Text>
                    </Row>
                    <Row justify="space-between">
                      <Text strong>Invoice Total</Text>
                      <Text strong>{formatNumber(invoice.totalAmount)}</Text>
                    </Row>
                  </Space>
                </Col>
              </Row>
            </Card>
          );
        })}

        <Card title="Daily Total" className="daily-total-card" style={{ marginTop: 20 }}>
          {dailyTotals.length === 0 ? (
            <Text type="secondary">No daily totals available.</Text>
          ) : (
            dailyTotals.map((total) => (
              <div key={total.currencyId} style={{ marginBottom: 12 }}>
                <Row justify="space-between">
                  <Text>Currency ID {total.currencyId}</Text>
                  <Text strong>{formatNumber(total.netSales)}</Text>
                </Row>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {total.invoiceCount} invoices
                  {" | "}
                  {total.productLineCount} product lines
                  {" | "}
                  Total quantity: {formatNumber(total.totalQuantity)}
                </Text>
              </div>
            ))
          )}
        </Card>
      </>
    );
  };

  const renderSalesItems = () => (
    <>
      <Card
        title="Sales Item Totals"
        size="small"
        style={{ marginBottom: 16 }}
        className="daily-total-card"
      >
        {itemTotals.length === 0 ? (
          <Text type="secondary">No sales item totals found.</Text>
        ) : (
          itemTotals.map((total) => (
            <div key={total.currencyId} style={{ marginBottom: 12 }}>
              <Row justify="space-between">
                <Text>Currency ID {total.currencyId}</Text>
                <Text strong>{formatNumber(total.netSales)}</Text>
              </Row>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {total.invoiceCount} vouchers
                {" | "}
                {total.productLineCount} product lines
                {" | "}
                Total quantity: {formatNumber(total.totalQuantity)}
                {" | "}
                Discount: {formatNumber(total.totalDiscount)}
                {" | "}
                Tax: {formatNumber(total.totalTax)}
              </Text>
            </div>
          ))
        )}
      </Card>

      <Table
        columns={columns}
        dataSource={data}
        rowKey="key"
        pagination={{
          pageSize: 25,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} product lines`,
        }}
        scroll={{ x: "max-content" }}
        size="small"
        bordered
        locale={{
          emptyText: "No posted sales products found for this date range.",
        }}
      />
    </>
  );

  return (
    <div className="custom-report-page">
      <style>{`
        @media print {
          body * {
            visibility: hidden !important;
          }

          .custom-report-print,
          .custom-report-print * {
            visibility: visible !important;
          }

          .custom-report-print {
            position: absolute !important;
            left: 0 !important;
            top: 0 !important;
            width: 100% !important;
            padding: 12px !important;
            background: white !important;
          }

          .no-print {
            display: none !important;
          }

          .invoice-detail-card,
          .daily-total-card {
            break-inside: avoid;
            page-break-inside: avoid;
          }

          .ant-table {
            font-size: 10px !important;
          }

          .ant-table table {
            width: 100% !important;
          }

          .ant-pagination {
            display: none !important;
          }

          @page {
            size: landscape;
            margin: 12mm;
          }
        }
      `}</style>

      <div className="no-print">
        <Title level={3}>Custom Report Builder</Title>
        <Text type="secondary">
          Choose a report, date range and columns, then preview or print.
        </Text>

        <Card style={{ marginTop: 16, marginBottom: 16 }}>
          <Row gutter={[16, 16]}>
            <Col xs={24} md={8}>
              <Text strong>Report Type</Text>
              <Select
                value={reportType}
                onChange={changeReportType}
                options={Object.entries(REPORT_TYPES).map(([value, label]) => ({
                  value,
                  label,
                }))}
                style={{ width: "100%", marginTop: 8 }}
              />
            </Col>

            <Col xs={24} md={10}>
              <Text strong>Date Range</Text>
              <RangePicker
                value={dateRange}
                onChange={setDateRange}
                allowClear
                style={{ width: "100%", marginTop: 8 }}
                format="YYYY-MM-DD"
              />
            </Col>

            <Col xs={24} md={6}>
              <Text strong>Actions</Text>
              <div style={{ marginTop: 8 }}>
                <Space wrap>
                  <Button
                    type="primary"
                    icon={<ReloadOutlined />}
                    onClick={loadReport}
                    loading={loading}
                  >
                    Preview
                  </Button>
                  <Button
                    icon={<PrinterOutlined />}
                    onClick={printReport}
                    disabled={!hasPreview}
                  >
                    Print / PDF
                  </Button>
                </Space>
              </div>
            </Col>
          </Row>

          <div style={{ marginTop: 20 }}>
            <Text strong>Select Columns</Text>
            <div style={{ marginTop: 10 }}>
              <Checkbox.Group
                value={selectedColumns}
                onChange={setSelectedColumns}
                options={REPORT_COLUMNS[reportType].map((column) => ({
                  label: column.title,
                  value: column.key,
                }))}
              />
            </div>
          </div>
        </Card>
      </div>

      {error && (
        <Alert
          className="no-print"
          type="error"
          showIcon
          message="Report Error"
          description={error}
          style={{ marginBottom: 16 }}
        />
      )}

      <div className="custom-report-print">
        <div style={{ marginBottom: 16 }}>
          <Title level={4}>{REPORT_TYPES[reportType]}</Title>
          <Text>
            Period: {dateRange?.[0]?.format("YYYY-MM-DD") ?? "-"}
            {" to "}
            {dateRange?.[1]?.format("YYYY-MM-DD") ?? "-"}
          </Text>
        </div>

        {loading ? (
          <div style={{ textAlign: "center", padding: 24 }}>
            <Spin />
          </div>
        ) : !hasPreview ? (
          <Alert
            type="info"
            showIcon
            message="Choose your filters and click Preview."
          />
        ) : selectedColumns.length === 0 ? (
          <Alert
            type="warning"
            showIcon
            message="Select at least one column to display the report."
          />
        ) : reportType === "daily" ? (
          renderInvoiceDetails()
        ) : reportType === "items" ? (
          renderSalesItems()
        ) : (
          <Table
            columns={columns}
            dataSource={data}
            rowKey="key"
            pagination={{ pageSize: 20, showSizeChanger: true }}
            scroll={{ x: "max-content" }}
            size="small"
            bordered
            locale={{ emptyText: "No report data found." }}
          />
        )}
      </div>
    </div>
  );
}
