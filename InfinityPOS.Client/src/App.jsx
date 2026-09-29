import { useState } from "react";
import Products from "./pages/products/Products";
import PurchaseInvoices from "./pages/purchase/PurchaseInvoices";
import PurchaseInvoiceForm from "./pages/purchase/PurchaseInvoiceForm";
import StockBalance from "./pages/stock/StockBalance";
import SalesInvoices from "./pages/sales/SalesInvoices";
import SalesInvoiceForm from "./pages/sales/SalesInvoiceForm";

import {
  Layout,
  Menu,
  Typography,
  Avatar,
  Dropdown,
  Card,
  Row,
  Col,
  Statistic,
  Space,
} from "antd";

import {
  DashboardOutlined,
  ShoppingCartOutlined,
  AppstoreOutlined,
  ShoppingOutlined,
  DatabaseOutlined,
  UserOutlined,
  TeamOutlined,
  DollarOutlined,
  BarChartOutlined,
  SettingOutlined,
  LogoutOutlined,
} from "@ant-design/icons";

import Login from "./pages/auth/Login";

import {
  getCurrentUser,
  isLoggedIn,
  logout,
} from "./api/authApi";

const { Header, Sider, Content } = Layout;
const { Title, Text } = Typography;

function Dashboard({ user, onLogout }) {
  const [selectedMenu, setSelectedMenu] =
    useState("dashboard");

  /* =========================
     PURCHASE STATE
  ========================= */

  const [purchasePage, setPurchasePage] =
    useState("list");

  const [editingPurchaseId, setEditingPurchaseId] =
    useState(null);

  /* =========================
     SALES STATE
  ========================= */

  const [salesPage, setSalesPage] =
    useState("list");

  const [editingSalesId, setEditingSalesId] =
    useState(null);

  /* =========================
     MENU
  ========================= */

  const menuItems = [
    {
      key: "dashboard",
      icon: <DashboardOutlined />,
      label: "Dashboard",
    },
    {
      key: "sales",
      icon: <ShoppingCartOutlined />,
      label: "Sales",
    },
    {
      key: "products",
      icon: <AppstoreOutlined />,
      label: "Products",
    },
    {
      key: "purchases",
      icon: <ShoppingOutlined />,
      label: "Purchases",
    },
    {
      key: "stock",
      icon: <DatabaseOutlined />,
      label: "Stock Balance",
    },
    {
      key: "customers",
      icon: <UserOutlined />,
      label: "Customers",
    },
    {
      key: "suppliers",
      icon: <TeamOutlined />,
      label: "Suppliers",
    },
    {
      key: "expenses",
      icon: <DollarOutlined />,
      label: "Expenses",
    },
    {
      key: "reports",
      icon: <BarChartOutlined />,
      label: "Reports",
    },
    {
      type: "divider",
    },
    {
      key: "settings",
      icon: <SettingOutlined />,
      label: "Settings",
    },
  ];

  const userMenuItems = [
    {
      key: "logout",
      icon: <LogoutOutlined />,
      label: "Logout",
      danger: true,
    },
  ];

  /* =========================
     USER MENU
  ========================= */

  const handleUserMenuClick = ({ key }) => {
    if (key === "logout") {
      onLogout();
    }
  };

  /* =========================
     MAIN MENU
  ========================= */

  const handleMenuClick = ({ key }) => {
    setSelectedMenu(key);

    if (key === "purchases") {
      setPurchasePage("list");
      setEditingPurchaseId(null);
    }

    if (key === "sales") {
      setSalesPage("list");
      setEditingSalesId(null);
    }
  };

  /* =========================
     PURCHASE HANDLERS
  ========================= */

  function handleNewPurchase() {
    setEditingPurchaseId(null);
    setPurchasePage("form");
  }

  function handleEditPurchase(
    purchaseInvoiceId
  ) {
    setEditingPurchaseId(
      purchaseInvoiceId
    );

    setPurchasePage("edit");
  }

  function handlePurchaseCancel() {
    setEditingPurchaseId(null);
    setPurchasePage("list");
  }

  function handlePurchaseSaved() {
    setEditingPurchaseId(null);
    setPurchasePage("list");
  }

  /* =========================
     SALES HANDLERS
  ========================= */

  function handleNewSale() {
    setEditingSalesId(null);
    setSalesPage("form");
  }

  function handleEditSale(
    salesInvoiceId
  ) {
    setEditingSalesId(
      salesInvoiceId
    );

    setSalesPage("edit");
  }

  function handleSalesCancel() {
    setEditingSalesId(null);
    setSalesPage("list");
  }

  function handleSalesSaved() {
    setEditingSalesId(null);
    setSalesPage("list");
  }

  return (
    <Layout
      style={{
        minHeight: "100vh",
      }}
    >
      {/* =========================
          SIDEBAR
      ========================= */}

      <Sider
        width={240}
        theme="light"
        breakpoint="lg"
        collapsedWidth="0"
      >
        <div
          style={{
            height: 64,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            borderBottom:
              "1px solid #f0f0f0",
          }}
        >
          <Title
            level={3}
            style={{
              margin: 0,
              color: "#1677ff",
            }}
          >
            InfinityPOS
          </Title>
        </div>

        <Menu
          mode="inline"
          selectedKeys={[selectedMenu]}
          items={menuItems}
          onClick={handleMenuClick}
          style={{
            borderRight: 0,
            marginTop: 12,
          }}
        />
      </Sider>

      <Layout>
        {/* =========================
            HEADER
        ========================= */}

        <Header
          style={{
            padding: "0 24px",
            background: "#ffffff",
            borderBottom:
              "1px solid #f0f0f0",
            display: "flex",
            alignItems: "center",
            justifyContent: "flex-end",
          }}
        >
          <Dropdown
            menu={{
              items: userMenuItems,
              onClick:
                handleUserMenuClick,
            }}
            placement="bottomRight"
          >
            <Space
              style={{
                cursor: "pointer",
              }}
            >
              <Avatar
                icon={<UserOutlined />}
                style={{
                  background: "#1677ff",
                }}
              />

              <div
                style={{
                  lineHeight: 1.2,
                }}
              >
                <Text strong>
                  {user?.displayName}
                </Text>

                <br />

                <Text
                  type="secondary"
                  style={{
                    fontSize: 12,
                  }}
                >
                  {user?.roleName}
                </Text>
              </div>
            </Space>
          </Dropdown>
        </Header>

        {/* =========================
            CONTENT
        ========================= */}

        <Content
          style={{
            padding: 24,
            background: "#f5f7fa",
          }}
        >
          {/* =========================
              DASHBOARD
          ========================= */}

          {selectedMenu === "dashboard" && (
            <>
              <div
                style={{
                  marginBottom: 24,
                }}
              >
                <Title
                  level={2}
                  style={{
                    marginBottom: 4,
                  }}
                >
                  Dashboard
                </Title>

                <Text type="secondary">
                  Welcome back,{" "}
                  {user?.displayName}
                </Text>
              </div>

              <Row gutter={[16, 16]}>
                <Col
                  xs={24}
                  sm={12}
                  lg={6}
                >
                  <Card>
                    <Statistic
                      title="Today's Sales"
                      value={0}
                      precision={2}
                      prefix="฿"
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
                      title="Today's Purchases"
                      value={0}
                      precision={2}
                      prefix="฿"
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
                      title="Today's Expenses"
                      value={0}
                      precision={2}
                      prefix="฿"
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
                      title="Stock Items"
                      value={0}
                    />
                  </Card>
                </Col>
              </Row>

              <Row
                gutter={[16, 16]}
                style={{
                  marginTop: 16,
                }}
              >
                <Col
                  xs={24}
                  lg={16}
                >
                  <Card title="Sales Overview">
                    <div
                      style={{
                        height: 280,
                        display: "flex",
                        alignItems:
                          "center",
                        justifyContent:
                          "center",
                        color: "#999",
                      }}
                    >
                      Sales chart will be
                      connected to the API
                      later.
                    </div>
                  </Card>
                </Col>

                <Col
                  xs={24}
                  lg={8}
                >
                  <Card title="Quick Summary">
                    <Space
                      orientation="vertical"
                      size="large"
                      style={{
                        width: "100%",
                      }}
                    >
                      <div>
                        <Text type="secondary">
                          Total Customers
                        </Text>

                        <br />

                        <Text
                          strong
                          style={{
                            fontSize: 24,
                          }}
                        >
                          0
                        </Text>
                      </div>

                      <div>
                        <Text type="secondary">
                          Total Suppliers
                        </Text>

                        <br />

                        <Text
                          strong
                          style={{
                            fontSize: 24,
                          }}
                        >
                          0
                        </Text>
                      </div>

                      <div>
                        <Text type="secondary">
                          Low Stock Items
                        </Text>

                        <br />

                        <Text
                          strong
                          style={{
                            fontSize: 24,
                          }}
                        >
                          0
                        </Text>
                      </div>
                    </Space>
                  </Card>
                </Col>
              </Row>

              <Card
                title="Recent Sales"
                style={{
                  marginTop: 16,
                }}
              >
                <div
                  style={{
                    minHeight: 180,
                    display: "flex",
                    alignItems:
                      "center",
                    justifyContent:
                      "center",
                    color: "#999",
                  }}
                >
                  Recent sales will appear
                  here.
                </div>
              </Card>
            </>
          )}

          {/* =========================
              SALES
          ========================= */}

          {selectedMenu === "sales" && (
            <>
              {salesPage === "list" && (
                <SalesInvoices
                  onNew={handleNewSale}
                  onEdit={handleEditSale}
                />
              )}

              {salesPage === "form" && (
                <SalesInvoiceForm
                  onCancel={
                    handleSalesCancel
                  }
                  onSaved={
                    handleSalesSaved
                  }
                />
              )}

              {salesPage === "edit" && (
                <SalesInvoiceForm
                  salesInvoiceId={
                    editingSalesId
                  }
                  onCancel={
                    handleSalesCancel
                  }
                  onSaved={
                    handleSalesSaved
                  }
                />
              )}
            </>
          )}

          {/* =========================
              PRODUCTS
          ========================= */}

          {selectedMenu === "products" && (
            <Products />
          )}

          {/* =========================
              PURCHASES
          ========================= */}

          {selectedMenu === "purchases" && (
            <>
              {purchasePage === "list" && (
                <PurchaseInvoices
                  onNewPurchase={
                    handleNewPurchase
                  }
                  onEditPurchase={
                    handleEditPurchase
                  }
                />
              )}

              {purchasePage === "form" && (
                <PurchaseInvoiceForm
                  onCancel={
                    handlePurchaseCancel
                  }
                  onSaved={
                    handlePurchaseSaved
                  }
                />
              )}

              {purchasePage === "edit" && (
                <PurchaseInvoiceForm
                  purchaseInvoiceId={
                    editingPurchaseId
                  }
                  onCancel={
                    handlePurchaseCancel
                  }
                  onSaved={
                    handlePurchaseSaved
                  }
                />
              )}
            </>
          )}

          {/* =========================
              STOCK BALANCE
          ========================= */}

          {selectedMenu === "stock" && (
            <StockBalance />
          )}

          {/* =========================
              OTHER MODULES
          ========================= */}

          {selectedMenu !== "dashboard" &&
            selectedMenu !== "sales" &&
            selectedMenu !== "products" &&
            selectedMenu !== "purchases" &&
            selectedMenu !== "stock" && (
              <Card>
                <Title level={3}>
                  {
                    menuItems.find(
                      (item) =>
                        item.key ===
                        selectedMenu
                    )?.label
                  }
                </Title>

                <Text type="secondary">
                  This module will be built
                  next.
                </Text>
              </Card>
            )}
        </Content>
      </Layout>
    </Layout>
  );
}

function App() {
  const [loggedIn, setLoggedIn] =
    useState(isLoggedIn());

  const [user, setUser] =
    useState(getCurrentUser());

  const handleLoginSuccess = (data) => {
    setUser(data);
    setLoggedIn(true);
  };

  const handleLogout = () => {
    logout();
    setUser(null);
    setLoggedIn(false);
  };

  if (!loggedIn) {
    return (
      <Login
        onLoginSuccess={
          handleLoginSuccess
        }
      />
    );
  }

  return (
    <Dashboard
      user={user}
      onLogout={handleLogout}
    />
  );
}

export default App;
