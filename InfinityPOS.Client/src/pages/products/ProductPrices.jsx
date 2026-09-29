import { useEffect, useState } from "react";
import {
  Table,
  Button,
  Space,
  Tag,
  InputNumber,
  Select,
  DatePicker,
  Popconfirm,
  message,
  Typography,
} from "antd";

import {
  PlusOutlined,
  DeleteOutlined,
  EditOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from "@ant-design/icons";

import dayjs from "dayjs";

import apiClient from "../../api/apiClient";
import {
  getProductPricesByProduct,
  createProductPrice,
  updateProductPrice,
  deleteProductPrice,
} from "../../api/productPricesApi";

const { Text } = Typography;

function ProductPrices({ productId }) {
  const [prices, setPrices] = useState([]);
  const [priceTypes, setPriceTypes] = useState([]);
  const [currencies, setCurrencies] = useState([]);

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const [editingId, setEditingId] = useState(null);

  const [formData, setFormData] = useState({
    priceTypeId: undefined,
    price: null,
    currencyId: 1,
    effectiveFrom: dayjs(),
    effectiveTo: null,
  });

  /*
   * -------------------------------------------------------
   * Load Price Data
   * -------------------------------------------------------
   */

  const loadData = async () => {
    if (!productId) return;

    try {
      setLoading(true);

      const pricesResponse =
        await getProductPricesByProduct(productId);

      const priceTypesResponse =
        await apiClient.get("/pricetypes");

      const currenciesResponse =
        await apiClient.get("/currencies");

      setPrices(
        Array.isArray(pricesResponse)
          ? pricesResponse
          : []
      );

      setPriceTypes(
        Array.isArray(priceTypesResponse.data)
          ? priceTypesResponse.data
          : []
      );

      setCurrencies(
        Array.isArray(currenciesResponse.data)
          ? currenciesResponse.data
          : []
      );
    } catch (error) {
      console.error(error);

      message.error(
        "Failed to load product price data."
      );
    } finally {
      setLoading(false);
    }
  };

  /*
   * -------------------------------------------------------
   * Load after component is mounted
   * -------------------------------------------------------
   */

  useEffect(() => {
    if (!productId) return;

    const timer = setTimeout(() => {
      loadData();
    }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [productId]);

  /*
   * -------------------------------------------------------
   * Reset Form
   * -------------------------------------------------------
   */

  const resetForm = () => {
    setEditingId(null);

    setFormData({
      priceTypeId: undefined,
      price: null,
      currencyId: 1,
      effectiveFrom: dayjs(),
      effectiveTo: null,
    });
  };

  /*
   * -------------------------------------------------------
   * Save Price
   * -------------------------------------------------------
   */

  const handleSave = async () => {
    if (!formData.priceTypeId) {
      message.warning(
        "Please select price type."
      );
      return;
    }

    if (
      formData.price === null ||
      formData.price === undefined
    ) {
      message.warning(
        "Please enter price."
      );
      return;
    }

    if (!formData.currencyId) {
      message.warning(
        "Please select currency."
      );
      return;
    }

    if (!formData.effectiveFrom) {
      message.warning(
        "Please select effective date."
      );
      return;
    }

    if (
      formData.effectiveTo &&
      formData.effectiveTo.isBefore(
        formData.effectiveFrom,
        "day"
      )
    ) {
      message.warning(
        "Effective To cannot be earlier than Effective From."
      );
      return;
    }

    try {
      setSaving(true);

      const priceData = {
        ProductId: productId,
        PriceTypeId: formData.priceTypeId,
        Price: formData.price,
        CurrencyId: formData.currencyId,

        EffectiveFrom:
          formData.effectiveFrom.toISOString(),

        EffectiveTo:
          formData.effectiveTo
            ? formData.effectiveTo.toISOString()
            : null,

        CreatedByUserId: null,
      };

      if (editingId) {
        await updateProductPrice(
          editingId,
          {
            ...priceData,
            IsActive: true,
          }
        );

        message.success(
          "Product price updated successfully."
        );
      } else {
        await createProductPrice(
          priceData
        );

        message.success(
          "Product price added successfully."
        );
      }

      resetForm();

      await loadData();
    } catch (error) {
      console.error(error);

      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.error ||
        error.response?.data ||
        "Failed to save product price.";

      message.error(
        typeof errorMessage === "string"
          ? errorMessage
          : "Failed to save product price."
      );
    } finally {
      setSaving(false);
    }
  };

  /*
   * -------------------------------------------------------
   * Edit Price
   * -------------------------------------------------------
   */

  const handleEdit = (record) => {
    setEditingId(
      record.productPriceId
    );

    setFormData({
      priceTypeId:
        record.priceTypeId,

      price:
        record.price,

      currencyId:
        record.currencyId,

      effectiveFrom:
        record.effectiveFrom
          ? dayjs(record.effectiveFrom)
          : dayjs(),

      effectiveTo:
        record.effectiveTo
          ? dayjs(record.effectiveTo)
          : null,
    });
  };

  /*
   * -------------------------------------------------------
   * Deactivate Price
   * -------------------------------------------------------
   */

  const handleDelete = async (id) => {
    try {
      await deleteProductPrice(id);

      message.success(
        "Product price deactivated successfully."
      );

      if (editingId === id) {
        resetForm();
      }

      await loadData();
    } catch (error) {
      console.error(error);

      message.error(
        "Failed to deactivate product price."
      );
    }
  };

  /*
   * -------------------------------------------------------
   * Price Type Helper
   * -------------------------------------------------------
   */

  const getPriceTypeId = (item) => {
    return (
      item?.priceTypeId ??
      item?.PriceTypeId ??
      item?.id ??
      item?.Id
    );
  };

  const getPriceTypeName = (id) => {
    const item = priceTypes.find(
      (x) =>
        getPriceTypeId(x) === id
    );

    if (!item) {
      return `Price Type ${id}`;
    }

    return (
      item.typeName ??
      item.TypeName ??
      item.priceTypeName ??
      item.PriceTypeName ??
      item.name ??
      item.Name ??
      item.code ??
      item.Code ??
      `Price Type ${id}`
    );
  };

  /*
   * -------------------------------------------------------
   * Currency Helper
   * -------------------------------------------------------
   */

  const getCurrencyId = (item) => {
    return (
      item?.currencyId ??
      item?.CurrencyId ??
      item?.id ??
      item?.Id
    );
  };

  const getCurrency = (id) => {
    return currencies.find(
      (x) =>
        getCurrencyId(x) === id
    );
  };

  const getCurrencyCode = (item) => {
    return (
      item?.code ??
      item?.Code ??
      item?.currencyCode ??
      item?.CurrencyCode ??
      ""
    );
  };

  const getCurrencyName = (item) => {
    return (
      item?.name ??
      item?.Name ??
      item?.currencyName ??
      item?.CurrencyName ??
      ""
    );
  };

  const getCurrencySymbol = (item) => {
    return (
      item?.symbol ??
      item?.Symbol ??
      item?.currencySymbol ??
      item?.CurrencySymbol ??
      ""
    );
  };

  /*
   * -------------------------------------------------------
   * Table Columns
   * -------------------------------------------------------
   */

  const columns = [
    {
      title: "Price Type",
      dataIndex: "priceTypeId",
      key: "priceTypeId",

      render: (id) => (
        <Text strong>
          {getPriceTypeName(id)}
        </Text>
      ),
    },

    {
      title: "Price",
      dataIndex: "price",
      key: "price",
      align: "right",

      render: (price, record) => {
        const currency =
          getCurrency(
            record.currencyId
          );

        const symbol =
          getCurrencySymbol(
            currency
          );

        return (
          <Text strong>
            {symbol}
            {Number(price).toLocaleString(
              undefined,
              {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              }
            )}
          </Text>
        );
      },
    },

    {
      title: "Effective From",
      dataIndex: "effectiveFrom",
      key: "effectiveFrom",

      render: (value) =>
        value
          ? dayjs(value).format(
              "DD/MM/YYYY"
            )
          : "-",
    },

    {
      title: "Effective To",
      dataIndex: "effectiveTo",
      key: "effectiveTo",

      render: (value) =>
        value
          ? dayjs(value).format(
              "DD/MM/YYYY"
            )
          : "No Expiry",
    },

    {
      title: "Status",
      dataIndex: "isActive",
      key: "isActive",

      render: (value) =>
        value ? (
          <Tag
            color="green"
            icon={
              <CheckCircleOutlined />
            }
          >
            Active
          </Tag>
        ) : (
          <Tag
            color="red"
            icon={
              <StopOutlined />
            }
          >
            Inactive
          </Tag>
        ),
    },

    {
      title: "Action",
      key: "action",

      render: (_, record) => (
        <Space>
          <Button
            size="small"
            icon={
              <EditOutlined />
            }
            onClick={() =>
              handleEdit(record)
            }
          >
            Edit
          </Button>

          <Popconfirm
            title="Deactivate this price?"
            description="This price will no longer be active."
            okText="Yes"
            cancelText="No"
            onConfirm={() =>
              handleDelete(
                record.productPriceId
              )
            }
          >
            <Button
              size="small"
              danger
              icon={
                <DeleteOutlined />
              }
            >
              Deactivate
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  /*
   * -------------------------------------------------------
   * Render
   * -------------------------------------------------------
   */

  return (
    <div>
      <div
        style={{
          marginBottom: 16,
          fontSize: 16,
          fontWeight: 600,
        }}
      >
        Product Prices
      </div>

      <div
        style={{
          display: "flex",
          gap: 12,
          alignItems: "flex-end",
          flexWrap: "wrap",
          marginBottom: 20,
          padding: 16,
          border:
            "1px solid #f0f0f0",
          borderRadius: 8,
        }}
      >
        {/* Price Type */}

        <div>
          <div
            style={{
              marginBottom: 6,
            }}
          >
            Price Type
          </div>

          <Select
            placeholder="Select price type"
            style={{
              width: 180,
            }}
            value={
              formData.priceTypeId
            }
            onChange={(value) =>
              setFormData({
                ...formData,
                priceTypeId: value,
              })
            }
            options={priceTypes.map(
              (item) => ({
                value:
                  getPriceTypeId(item),

                label:
                  getPriceTypeName(
                    getPriceTypeId(
                      item
                    )
                  ),
              })
            )}
          />
        </div>

        {/* Currency */}

        <div>
          <div
            style={{
              marginBottom: 6,
            }}
          >
            Currency
          </div>

          <Select
            style={{
              width: 180,
            }}
            value={
              formData.currencyId
            }
            onChange={(value) =>
              setFormData({
                ...formData,
                currencyId: value,
              })
            }
            options={currencies.map(
              (item) => {
                const code =
                  getCurrencyCode(
                    item
                  );

                const name =
                  getCurrencyName(
                    item
                  );

                const symbol =
                  getCurrencySymbol(
                    item
                  );

                return {
                  value:
                    getCurrencyId(
                      item
                    ),

                  label: `${code || "Currency"}${
                    name
                      ? ` - ${name}`
                      : ""
                  }${
                    symbol
                      ? ` (${symbol})`
                      : ""
                  }`,
                };
              }
            )}
          />
        </div>

        {/* Price */}

        <div>
          <div
            style={{
              marginBottom: 6,
            }}
          >
            Price
          </div>

          <InputNumber
            min={0}
            precision={2}
            placeholder="Enter price"
            style={{
              width: 160,
            }}
            value={
              formData.price
            }
            onChange={(value) =>
              setFormData({
                ...formData,
                price: value,
              })
            }
          />
        </div>

        {/* Effective From */}

        <div>
          <div
            style={{
              marginBottom: 6,
            }}
          >
            Effective From
          </div>

          <DatePicker
            value={
              formData.effectiveFrom
            }
            onChange={(value) =>
              setFormData({
                ...formData,
                effectiveFrom:
                  value,
              })
            }
            format="DD/MM/YYYY"
          />
        </div>

        {/* Effective To */}

        <div>
          <div
            style={{
              marginBottom: 6,
            }}
          >
            Effective To
          </div>

          <DatePicker
            value={
              formData.effectiveTo
            }
            onChange={(value) =>
              setFormData({
                ...formData,
                effectiveTo:
                  value,
              })
            }
            format="DD/MM/YYYY"
            allowClear
          />
        </div>

        {/* Buttons */}

        <Space>
          <Button
            type="primary"
            icon={
              <PlusOutlined />
            }
            onClick={
              handleSave
            }
            loading={saving}
          >
            {editingId
              ? "Update"
              : "Add Price"}
          </Button>

          {editingId && (
            <Button
              onClick={
                resetForm
              }
            >
              Cancel
            </Button>
          )}
        </Space>
      </div>

      {/* Price Table */}

      <Table
        rowKey="productPriceId"
        columns={columns}
        dataSource={prices}
        loading={loading}
        pagination={false}
        scroll={{
          x: 900,
        }}
      />
    </div>
  );
}

export default ProductPrices;
