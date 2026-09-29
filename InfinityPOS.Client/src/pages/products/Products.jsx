import "./Products.css";
import { useEffect, useState } from "react";
import {
  Card,
  Table,
  Button,
  Space,
  Input,
  Typography,
  message,
  Tag,
  Segmented,
} from "antd";

import {
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
  ArrowLeftOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from "@ant-design/icons";

import { getProducts } from "../../api/productsApi";
import apiClient from "../../api/apiClient";
import ProductForm from "./ProductForm";

import "./Products.css";

const { Title, Text } = Typography;

function Products() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [units, setUnits] = useState([]);

  const [loading, setLoading] = useState(false);
  const [searchText, setSearchText] = useState("");

  const [productStatus, setProductStatus] = useState("active");

  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);

  const loadData = async () => {
    try {
      setLoading(true);

      const isActive = productStatus === "active";

      const [
        productsData,
        categoriesResponse,
        brandsResponse,
        unitsResponse,
      ] = await Promise.all([
        getProducts(isActive),
        apiClient.get("/categories"),
        apiClient.get("/brands"),
        apiClient.get("/units"),
      ]);

      setProducts(productsData);
      setCategories(categoriesResponse.data);
      setBrands(brandsResponse.data);
      setUnits(unitsResponse.data);
    } catch (error) {
      console.error(error);
      message.error("Failed to load product data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      loadData();
    }, 0);

    return () => clearTimeout(timer);
  }, [productStatus]);

  const getCategoryName = (categoryId) => {
    const category = categories.find(
      (item) => item.categoryId === categoryId
    );

    return category?.categoryName || "-";
  };

  const getBrandName = (brandId) => {
    const brand = brands.find(
      (item) => item.brandId === brandId
    );

    return brand?.brandName || "-";
  };

  const getUnitName = (unitId) => {
    const unit = units.find(
      (item) => item.unitId === unitId
    );

    return unit?.unitName || "-";
  };

  const filteredProducts = products.filter((product) => {
    const search = searchText.trim().toLowerCase();

    if (!search) {
      return true;
    }

    return (
      product.productName?.toLowerCase().includes(search) ||
      product.sku?.toLowerCase().includes(search) ||
      product.barcode?.toLowerCase().includes(search)
    );
  });

  const handleAddProduct = () => {
    setEditingProduct(null);
    setShowForm(true);
  };

  const handleEditProduct = (product) => {
    setEditingProduct(product);
    setShowForm(true);
  };

  const handleFormSuccess = async () => {
    setShowForm(false);
    setEditingProduct(null);

    await loadData();
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingProduct(null);
  };

  const columns = [
    {
      title: "SKU",
      dataIndex: "sku",
      key: "sku",
      sorter: (a, b) =>
        (a.sku || "").localeCompare(
          b.sku || "",
          undefined,
          {
            numeric: true,
            sensitivity: "base",
          }
        ),
    },

    {
      title: "Barcode",
      dataIndex: "barcode",
      key: "barcode",
      sorter: (a, b) =>
        (a.barcode || "").localeCompare(
          b.barcode || "",
          undefined,
          {
            numeric: true,
            sensitivity: "base",
          }
        ),
    },

    {
      title: "Product Name",
      dataIndex: "productName",
      key: "productName",
      sorter: (a, b) =>
        (a.productName || "").localeCompare(
          b.productName || "",
          undefined,
          {
            sensitivity: "base",
          }
        ),
      render: (text) => <Text strong>{text}</Text>,
    },

    {
      title: "Category",
      dataIndex: "categoryId",
      key: "categoryId",
      sorter: (a, b) =>
        getCategoryName(a.categoryId).localeCompare(
          getCategoryName(b.categoryId),
          undefined,
          {
            sensitivity: "base",
          }
        ),
      render: (categoryId) =>
        getCategoryName(categoryId),
    },

    {
      title: "Brand",
      dataIndex: "brandId",
      key: "brandId",
      sorter: (a, b) =>
        getBrandName(a.brandId).localeCompare(
          getBrandName(b.brandId),
          undefined,
          {
            sensitivity: "base",
          }
        ),
      render: (brandId) =>
        getBrandName(brandId),
    },

    {
      title: "Unit",
      dataIndex: "unitId",
      key: "unitId",
      sorter: (a, b) =>
        getUnitName(a.unitId).localeCompare(
          getUnitName(b.unitId),
          undefined,
          {
            sensitivity: "base",
          }
        ),
      render: (unitId) =>
        getUnitName(unitId),
    },

    {
      title: "Inventory",
      dataIndex: "trackInventory",
      key: "trackInventory",
      sorter: (a, b) =>
        Number(a.trackInventory) -
        Number(b.trackInventory),
      render: (value) =>
        value ? (
          <Tag color="green">
            Tracked
          </Tag>
        ) : (
          <Tag>
            Not Tracked
          </Tag>
        ),
    },

    {
      title: "Status",
      dataIndex: "isActive",
      key: "isActive",
      sorter: (a, b) =>
        Number(a.isActive) -
        Number(b.isActive),
      render: (value) =>
        value ? (
          <Tag
            color="green"
            icon={<CheckCircleOutlined />}
          >
            Active
          </Tag>
        ) : (
          <Tag
            color="red"
            icon={<StopOutlined />}
          >
            Inactive
          </Tag>
        ),
    },
  ];

  if (showForm) {
    return (
      <div>
        <Card>
          <Button
            icon={<ArrowLeftOutlined />}
            onClick={handleFormCancel}
            style={{ marginBottom: 24 }}
          >
            Back to Products
          </Button>

          <ProductForm
            product={editingProduct}
            onSuccess={handleFormSuccess}
            onCancel={handleFormCancel}
          />
        </Card>
      </div>
    );
  }

  const activeLabel = (
    <Space>
      <CheckCircleOutlined />
      Active Products
    </Space>
  );

  const inactiveLabel = (
    <Space>
      <StopOutlined />
      Inactive Products
    </Space>
  );

  return (
    <div>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: 24,
          gap: 16,
          flexWrap: "wrap",
        }}
      >
        <div>
          <Title
            level={2}
            style={{ marginBottom: 4 }}
          >
            Products
          </Title>

          <Text type="secondary">
            Manage your products and inventory items.
          </Text>
        </div>

        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleAddProduct}
        >
          Add Product
        </Button>
      </div>

      <Card>
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: 20,
            gap: 16,
            flexWrap: "wrap",
          }}
        >
          <Segmented
            value={productStatus}
            onChange={(value) => {
              setProductStatus(value);
              setSearchText("");
            }}
            options={[
              {
                label: activeLabel,
                value: "active",
              },
              {
                label: inactiveLabel,
                value: "inactive",
              },
            ]}
          />

          <Space>
            <Text type="secondary">
              {filteredProducts.length} products
            </Text>

            <Button
              icon={<ReloadOutlined />}
              onClick={loadData}
              loading={loading}
            >
              Refresh
            </Button>
          </Space>
        </div>

        <div style={{ marginBottom: 16 }}>
          <Input
            allowClear
            prefix={<SearchOutlined />}
            placeholder="Search SKU, barcode or product name..."
            value={searchText}
            onChange={(e) =>
              setSearchText(e.target.value)
            }
            style={{ maxWidth: 400 }}
          />
        </div>

        <Table
          rowKey="productId"
          columns={columns}
          dataSource={filteredProducts}
          loading={loading}
          scroll={{ x: 1000 }}
          onRow={(record) => ({
            onDoubleClick: () =>
              handleEditProduct(record),

            className: "product-table-row",
          })}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} products`,
          }}
        />
      </Card>
    </div>
  );
}

export default Products;