import { useEffect, useState } from "react";
import {
  Form,
  Input,
  Select,
  Switch,
  Button,
  Space,
  message,
  Divider,
  Typography,
} from "antd";

import {
  SaveOutlined,
  ClearOutlined,
} from "@ant-design/icons";

import apiClient from "../../api/apiClient";

import {
  createProduct,
  updateProduct,
} from "../../api/productsApi";

import ProductPrices from "./ProductPrices";

const { Title } = Typography;

function ProductForm({
  product = null,
  onSuccess,
  onCancel,
}) {
  const [form] = Form.useForm();

  const [categories, setCategories] = useState([]);
  const [brands, setBrands] = useState([]);
  const [units, setUnits] = useState([]);
  const [productTypes, setProductTypes] = useState([]);

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] =
    useState(false);

  const isEditMode = Boolean(product);

  useEffect(() => {
    const loadOptions = async () => {
      try {
        setLoadingData(true);

        const [
          categoriesResponse,
          brandsResponse,
          unitsResponse,
          productTypesResponse,
        ] = await Promise.all([
          apiClient.get("/categories"),
          apiClient.get("/brands"),
          apiClient.get("/units"),
          apiClient.get("/producttypes"),
        ]);

        setCategories(categoriesResponse.data);
        setBrands(brandsResponse.data);
        setUnits(unitsResponse.data);
        setProductTypes(
          productTypesResponse.data
        );
      } catch (error) {
        console.error(error);

        message.error(
          "Failed to load product options."
        );
      } finally {
        setLoadingData(false);
      }
    };

    loadOptions();
  }, []);

  useEffect(() => {
    if (!product) {
      form.resetFields();

      form.setFieldsValue({
        TrackInventory: true,
        AllowNegativeStock: false,
      });

      return;
    }

    form.setFieldsValue({
      SKU: product.sku,
      Barcode: product.barcode || "",
      ProductName: product.productName,

      CategoryId:
        product.categoryId ?? undefined,

      BrandId:
        product.brandId ?? undefined,

      ProductTypeId:
        product.productTypeId,

      UnitId:
        product.unitId,

      Description:
        product.description || "",

      TrackInventory:
        product.trackInventory,

      AllowNegativeStock:
        product.allowNegativeStock,

      IsActive:
        product.isActive,
    });
  }, [product, form]);

  const handleSubmit = async (values) => {
    try {
      setLoading(true);

      if (isEditMode) {
        const productData = {
          SKU: values.SKU.trim(),

          Barcode:
            values.Barcode?.trim() || null,

          ProductName:
            values.ProductName.trim(),

          CategoryId:
            values.CategoryId ?? null,

          BrandId:
            values.BrandId ?? null,

          ProductTypeId:
            values.ProductTypeId,

          UnitId:
            values.UnitId,

          Description:
            values.Description?.trim() ||
            null,

          TrackInventory:
            values.TrackInventory,

          AllowNegativeStock:
            values.AllowNegativeStock,

          IsActive:
            values.IsActive,
        };

        await updateProduct(
          product.productId,
          productData
        );

        message.success(
          "Product updated successfully."
        );
      } else {
        const productData = {
          SKU: values.SKU.trim(),

          Barcode:
            values.Barcode?.trim() || null,

          ProductName:
            values.ProductName.trim(),

          CategoryId:
            values.CategoryId ?? null,

          BrandId:
            values.BrandId ?? null,

          ProductTypeId:
            values.ProductTypeId,

          UnitId:
            values.UnitId,

          Description:
            values.Description?.trim() ||
            null,

          TrackInventory:
            values.TrackInventory,

          AllowNegativeStock:
            values.AllowNegativeStock,
        };

        await createProduct(productData);

        message.success(
          "Product created successfully."
        );
      }

      if (onSuccess) {
        await onSuccess();
      }
    } catch (error) {
      console.error(error);

      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.error ||
        "Failed to save product.";

      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    if (isEditMode && product) {
      form.setFieldsValue({
        SKU: product.sku,

        Barcode:
          product.barcode || "",

        ProductName:
          product.productName,

        CategoryId:
          product.categoryId ??
          undefined,

        BrandId:
          product.brandId ??
          undefined,

        ProductTypeId:
          product.productTypeId,

        UnitId:
          product.unitId,

        Description:
          product.description || "",

        TrackInventory:
          product.trackInventory,

        AllowNegativeStock:
          product.allowNegativeStock,

        IsActive:
          product.isActive,
      });
    } else {
      form.resetFields();

      form.setFieldsValue({
        TrackInventory: true,
        AllowNegativeStock: false,
      });
    }
  };

  return (
    <div>
      <Title level={3}>
        {isEditMode
          ? "Edit Product"
          : "Add Product"}
      </Title>

      <Divider />

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        disabled={loadingData}
        onKeyDown={(e) => {
          if (
            e.key === "Enter" &&
            e.target?.closest?.(".ant-input-number")
          ) {
            e.preventDefault();
            e.stopPropagation();
          }
        }}
      >
        <Form.Item
          label="SKU"
          name="SKU"
          rules={[
            {
              required: true,
              message:
                "Please enter SKU.",
            },
          ]}
        >
          <Input
            placeholder="e.g. CASE-001"
            maxLength={100}
          />
        </Form.Item>

        <Form.Item
          label="Barcode"
          name="Barcode"
        >
          <Input
            placeholder="Enter barcode"
            maxLength={100}
          />
        </Form.Item>

        <Form.Item
          label="Product Name"
          name="ProductName"
          rules={[
            {
              required: true,
              message:
                "Please enter product name.",
            },
          ]}
        >
          <Input
            placeholder="Enter product name"
            maxLength={200}
          />
        </Form.Item>

        <Form.Item
          label="Category"
          name="CategoryId"
        >
          <Select
            allowClear
            placeholder="Select category"
            loading={loadingData}
            options={categories.map(
              (item) => ({
                value:
                  item.categoryId,
                label:
                  item.categoryName,
              })
            )}
          />
        </Form.Item>

        <Form.Item
          label="Brand"
          name="BrandId"
        >
          <Select
            allowClear
            placeholder="Select brand"
            loading={loadingData}
            options={brands.map(
              (item) => ({
                value:
                  item.brandId,
                label:
                  item.brandName,
              })
            )}
          />
        </Form.Item>

        <Form.Item
          label="Product Type"
          name="ProductTypeId"
          rules={[
            {
              required: true,
              message:
                "Please select product type.",
            },
          ]}
        >
          <Select
            placeholder="Select product type"
            loading={loadingData}
            options={productTypes.map(
              (item) => ({
                value:
                  item.productTypeId,
                label:
                  item.typeName,
              })
            )}
          />
        </Form.Item>

        <Form.Item
          label="Unit"
          name="UnitId"
          rules={[
            {
              required: true,
              message:
                "Please select unit.",
            },
          ]}
        >
          <Select
            placeholder="Select unit"
            loading={loadingData}
            options={units.map(
              (item) => ({
                value: item.unitId,
                label: item.unitName,
              })
            )}
          />
        </Form.Item>

        <Form.Item
          label="Description"
          name="Description"
        >
          <Input.TextArea
            rows={4}
            placeholder="Enter product description"
          />
        </Form.Item>

        <Form.Item
          label="Track Inventory"
          name="TrackInventory"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>

        <Form.Item
          label="Allow Negative Stock"
          name="AllowNegativeStock"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>

        {isEditMode && (
          <Form.Item
            label="Active"
            name="IsActive"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>
        )}

        {isEditMode && (
          <>
            <Divider />

            <ProductPrices
              productId={
                product.productId
              }
            />
          </>
        )}

        <Divider />

        <Space>
          <Button
            type="primary"
            htmlType="submit"
            icon={<SaveOutlined />}
            loading={loading}
          >
            {isEditMode
              ? "Update Product"
              : "Save Product"}
          </Button>

          <Button
            icon={<ClearOutlined />}
            onClick={handleReset}
            disabled={loading}
          >
            Reset
          </Button>

          {onCancel && (
            <Button
              onClick={onCancel}
              disabled={loading}
            >
              Cancel
            </Button>
          )}
        </Space>
      </Form>
    </div>
  );
}

export default ProductForm;