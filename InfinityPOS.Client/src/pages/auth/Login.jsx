import { useState } from "react";
import {
  Button,
  Card,
  Form,
  Input,
  Typography,
  message,
} from "antd";
import {
  LockOutlined,
  UserOutlined,
} from "@ant-design/icons";

import { login, saveLoginSession } from "../../api/authApi";

const { Title, Text } = Typography;

function Login({ onLoginSuccess }) {
  const [loading, setLoading] = useState(false);

  const handleLogin = async (values) => {
    try {
      setLoading(true);

      const data = await login(
        values.username,
        values.password
      );

      saveLoginSession(data);

      message.success("Login successful.");

      // console.log("Logged in user:", data);

      onLoginSuccess(data);
    } catch (error) {
      const errorMessage =
        error.response?.data?.message ||
        "Login failed. Please check username and password.";

      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "#f5f7fa",
        padding: 24,
      }}
    >
      <Card
        style={{
          width: "100%",
          maxWidth: 420,
          borderRadius: 16,
        }}
      >
        <div
          style={{
            textAlign: "center",
            marginBottom: 32,
          }}
        >
          <Title
            level={2}
            style={{ marginBottom: 8 }}
          >
            InfinityPOS
          </Title>

          <Text type="secondary">
            Point of Sale System
          </Text>
        </div>

        <Form
          layout="vertical"
          onFinish={handleLogin}
          autoComplete="off"
        >
          <Form.Item
            label="Username"
            name="username"
            rules={[
              {
                required: true,
                message: "Please enter username.",
              },
            ]}
          >
            <Input
              size="large"
              prefix={<UserOutlined />}
              placeholder="Enter username"
            />
          </Form.Item>

          <Form.Item
            label="Password"
            name="password"
            rules={[
              {
                required: true,
                message: "Please enter password.",
              },
            ]}
          >
            <Input.Password
              size="large"
              prefix={<LockOutlined />}
              placeholder="Enter password"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0 }}>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              block
              loading={loading}
            >
              Login
            </Button>
          </Form.Item>
        </Form>

        <div
          style={{
            textAlign: "center",
            marginTop: 24,
          }}
        >
          <Text type="secondary">
            InfinityPOS © 2026
          </Text>
        </div>
      </Card>
    </div>
  );
}

export default Login;
