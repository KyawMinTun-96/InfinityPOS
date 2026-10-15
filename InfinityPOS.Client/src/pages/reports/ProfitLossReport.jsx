import { Card, DatePicker, Row, Col, Typography, Button, Space } from "antd";
import { useState } from "react";

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

function ProfitLossReport() {
  const [dateRange, setDateRange] = useState(null);

  return (
    <div>
      <Title level={2}>Profit & Loss Report</Title>

      <Text type="secondary">
        Accounting Statement
      </Text>

      <Card style={{ marginTop: 20 }}>
        <Space direction="vertical" size="middle" style={{ width: "100%" }}>
          <Text strong>Report Period</Text>

          <RangePicker
            style={{ width: "100%", maxWidth: 420 }}
            format="YYYY-MM-DD"
            onChange={(dates) => setDateRange(dates)}
          />

          <Button
            type="primary"
            onClick={() => {
              if (!dateRange) {
                return;
              }

              // API connection will be added after
              // the database schema has been verified.
            }}
            disabled={!dateRange}
          >
            Generate Report
          </Button>
        </Space>
      </Card>

      <Row gutter={[16, 16]} style={{ marginTop: 20 }}>
        <Col xs={24} md={12} xl={8}>
          <Card title="Revenue">
            <Title level={3}>—</Title>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={8}>
          <Card title="Cost of Goods Sold (COGS)">
            <Title level={3}>—</Title>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={8}>
          <Card title="Gross Profit">
            <Title level={3}>—</Title>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={8}>
          <Card title="Expenses">
            <Title level={3}>—</Title>
          </Card>
        </Col>

        <Col xs={24} md={12} xl={8}>
          <Card title="Net Profit">
            <Title level={3}>—</Title>
          </Card>
        </Col>
      </Row>

      <Card style={{ marginTop: 20 }}>
        <Text type="secondary">
          Profit & Loss totals will appear after the report API
          is connected to the verified database schema.
        </Text>
      </Card>
    </div>
  );
}

export default ProfitLossReport;
