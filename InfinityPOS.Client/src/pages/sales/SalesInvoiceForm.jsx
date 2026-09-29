import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";

import {
  Button,
  Card,
  Col,
  DatePicker,
  Divider,
  Form,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Table,
  Typography,
  message,
} from "antd";

import {
  DeleteOutlined,
  PlusOutlined,
  PrinterOutlined,
  CheckOutlined,
} from "@ant-design/icons";

import dayjs from "dayjs";

import {
  getSalesInvoice,
  createSalesInvoice,
  postSalesInvoice,
} from "../../api/salesInvoicesApi";

import {
  getSalesInvoiceItemsByInvoice,
  createSalesInvoiceItem,
  updateSalesInvoiceItem,
  deleteSalesInvoiceItem,
} from "../../api/salesInvoiceItemsApi";

import {
  getProducts,
} from "../../api/productsApi";

import {
  getCustomers,
} from "../../api/customersApi";

import {
  getWarehouses,
} from "../../api/warehousesApi";

import {
  getCurrencies,
} from "../../api/currenciesApi";

import {
  getProductStockBatches,
} from "../../api/productStockBatchesApi";

import {
  createSalesPayment,
} from "../../api/salesPaymentsApi";

const { Title, Text } = Typography;

/* =========================================================
   COMMON HELPERS
========================================================= */

function getValue(object, ...keys) {
  for (const key of keys) {
    if (
      object &&
      object[key] !== undefined &&
      object[key] !== null
    ) {
      return object[key];
    }
  }

  return null;
}

function safeNumber(value) {
  const number = Number(value);

  return Number.isFinite(number)
    ? number
    : 0;
}

function formatMoney(value) {
  return safeNumber(value).toLocaleString(
    "en-US",
    {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }
  );
}

/* =========================================================
   PRODUCT HELPERS
========================================================= */

function getProductId(product) {
  return Number(
    getValue(
      product,
      "productId",
      "ProductId"
    )
  );
}

function getProductName(product) {
  return (
    getValue(
      product,
      "productName",
      "ProductName",
      "name",
      "Name"
    ) ||
    `Product #${getProductId(product)}`
  );
}

function getProductCode(product) {
  return (
    getValue(
      product,
      "productCode",
      "ProductCode",
      "code",
      "Code",
      "sku",
      "SKU",
      "itemCode",
      "ItemCode"
    ) || ""
  );
}

function getProductBarcode(product) {
  return (
    getValue(
      product,
      "barcode",
      "Barcode",
      "barCode",
      "BarCode",
      "productBarcode",
      "ProductBarcode",
      "productBarCode",
      "ProductBarCode"
    ) || ""
  );
}

/* =========================================================
   CUSTOMER HELPERS
========================================================= */

function getCustomerId(customer) {
  return Number(
    getValue(
      customer,
      "customerId",
      "CustomerId"
    )
  );
}

function getCustomerName(customer) {
  return (
    getValue(
      customer,
      "customerName",
      "CustomerName",
      "name",
      "Name"
    ) ||
    `Customer #${getCustomerId(customer)}`
  );
}

/* =========================================================
   WAREHOUSE HELPERS
========================================================= */

function getWarehouseId(warehouse) {
  return Number(
    getValue(
      warehouse,
      "warehouseId",
      "WarehouseId"
    )
  );
}

function getWarehouseName(warehouse) {
  return (
    getValue(
      warehouse,
      "warehouseName",
      "WarehouseName",
      "name",
      "Name"
    ) ||
    `Warehouse #${getWarehouseId(
      warehouse
    )}`
  );
}

/* =========================================================
   CURRENCY HELPERS
========================================================= */

function getCurrencyId(currency) {
  return Number(
    getValue(
      currency,
      "currencyId",
      "CurrencyId"
    )
  );
}

function getCurrencyName(currency) {
  return (
    getValue(
      currency,
      "currencyName",
      "CurrencyName",
      "name",
      "Name"
    ) ||
    getValue(
      currency,
      "currencyCode",
      "CurrencyCode",
      "code",
      "Code"
    ) ||
    `Currency #${getCurrencyId(
      currency
    )}`
  );
}

/* =========================================================
   STOCK BATCH HELPERS
========================================================= */

function getStockBatchId(batch) {
  const value = getValue(
    batch,
    "productStockBatchId",
    "ProductStockBatchId"
  );

  return value
    ? Number(value)
    : null;
}

function getBatchSalePrice(batch) {
  return safeNumber(
    getValue(
      batch,
      "salePrice",
      "SalePrice"
    )
  );
}

function getBatchCostPrice(batch) {
  return safeNumber(
    getValue(
      batch,
      "costPrice",
      "CostPrice"
    )
  );
}

function getBatchRemainingQuantity(batch) {
  return safeNumber(
    getValue(
      batch,
      "remainingQuantity",
      "RemainingQuantity"
    )
  );
}

/* =========================================================
   COMPONENT
========================================================= */

export default function SalesInvoiceForm({
  salesInvoiceId,
  onCancel,
  onSaved,
}) {
  const [form] = Form.useForm();

  const [
    messageApi,
    contextHolder,
  ] = message.useMessage();

  const isEditMode =
    Boolean(salesInvoiceId);

  const [loading, setLoading] =
    useState(false);

  const [products, setProducts] =
    useState([]);

  const [customers, setCustomers] =
    useState([]);

  const [warehouses, setWarehouses] =
    useState([]);

  const [currencies, setCurrencies] =
    useState([]);

  const [items, setItems] =
    useState([]);

  const [stockBatches, setStockBatches] =
    useState([]);

  const [
    selectedProductId,
    setSelectedProductId,
  ] = useState(null);

  const [
    selectedStockBatchId,
    setSelectedStockBatchId,
  ] = useState(null);

  const [
    selectedQuantity,
    setSelectedQuantity,
  ] = useState(1);

  const [
    selectedUnitPrice,
    setSelectedUnitPrice,
  ] = useState(0);

  const [
    selectedUnitCost,
    setSelectedUnitCost,
  ] = useState(0);

  const [
    selectedDiscount,
    setSelectedDiscount,
  ] = useState(0);

  const [
    selectedTax,
    setSelectedTax,
  ] = useState(0);

  const [
    salesInvoiceIdState,
    setSalesInvoiceIdState,
  ] = useState(
    salesInvoiceId || null
  );

  const [
    isCompleted,
    setIsCompleted,
  ] = useState(false);

  const [
    paidAmount,
    setPaidAmount,
  ] = useState(0);

  const [
    paymentMethodId,
    setPaymentMethodId,
  ] = useState(1);

  const exchangeRate =
    Form.useWatch(
      "exchangeRate",
      form
    );

  const customerId =
    Form.useWatch(
      "customerId",
      form
    );

  const warehouseId =
    Form.useWatch(
      "warehouseId",
      form
    );

  const currencyId =
    Form.useWatch(
      "currencyId",
      form
    );

  /* =========================================================
     TOTALS
  ========================================================= */

  const totals = useMemo(() => {
    let subTotal = 0;
    let discountAmount = 0;
    let taxAmount = 0;

    for (const item of items) {
      subTotal +=
        safeNumber(item.quantity) *
        safeNumber(item.unitPrice);

      discountAmount +=
        safeNumber(
          item.discountAmount
        );

      taxAmount +=
        safeNumber(
          item.taxAmount
        );
    }

    const totalAmount =
      subTotal -
      discountAmount +
      taxAmount;

    return {
      subTotal,
      discountAmount,
      taxAmount,
      totalAmount:
        totalAmount < 0
          ? 0
          : totalAmount,
    };
  }, [items]);

  /* =========================================================
     PAYMENT CALCULATIONS
  ========================================================= */

  const balanceAmount = useMemo(() => {
    return Math.max(
      0,
      totals.totalAmount -
        safeNumber(paidAmount)
    );
  }, [
    paidAmount,
    totals.totalAmount,
  ]);

  const changeAmount = useMemo(() => {
    return Math.max(
      0,
      safeNumber(paidAmount) -
        totals.totalAmount
    );
  }, [
    paidAmount,
    totals.totalAmount,
  ]);

  /* =========================================================
     LOAD MASTER DATA
  ========================================================= */

  const loadMasterData =
    useCallback(async () => {
      try {
        const [
          productData,
          customerData,
          warehouseData,
          currencyData,
        ] = await Promise.all([
          getProducts(true),
          getCustomers(true),
          getWarehouses(true),
          getCurrencies(true),
        ]);

        setProducts(
          Array.isArray(productData)
            ? productData
            : []
        );

        setCustomers(
          Array.isArray(customerData)
            ? customerData
            : []
        );

        setWarehouses(
          Array.isArray(warehouseData)
            ? warehouseData
            : []
        );

        setCurrencies(
          Array.isArray(currencyData)
            ? currencyData
            : []
        );
      } catch (error) {
        console.error(
          "FAILED TO LOAD SALES MASTER DATA:",
          error
        );

        console.error(
          "API RESPONSE:",
          error?.response?.data
        );

        messageApi.error(
          "Failed to load sales master data."
        );
      }
    }, [messageApi]);

  /* =========================================================
     LOAD PRODUCT STOCK BATCHES
  ========================================================= */

  const loadProductStockBatches =
    useCallback(
      async (
        productId,
        selectedWarehouseId
      ) => {
        if (
          !productId ||
          !selectedWarehouseId
        ) {
          setStockBatches([]);
          return [];
        }

        try {
          const data =
            await getProductStockBatches(
              Number(productId),
              Number(selectedWarehouseId)
            );

          const batches =
            Array.isArray(data)
              ? data
              : [];

          setStockBatches(batches);

          return batches;
        } catch (error) {
          console.error(
            "FAILED TO LOAD PRODUCT STOCK BATCHES:",
            error
          );

          console.error(
            "API RESPONSE:",
            error?.response?.data
          );

          setStockBatches([]);

          messageApi.error(
            error?.response?.data
              ?.message ||
              "Failed to load product stock batches."
          );

          return [];
        }
      },
      [messageApi]
    );

  /* =========================================================
     LOAD EDIT DATA
  ========================================================= */

  const loadEditData =
    useCallback(
      async (id) => {
        if (!id) {
          return;
        }

        try {
          setLoading(true);

          const [
            invoice,
            invoiceItems,
          ] = await Promise.all([
            getSalesInvoice(id),
            getSalesInvoiceItemsByInvoice(id),
          ]);

          const invoiceDate =
            getValue(
              invoice,
              "invoiceDate",
              "InvoiceDate"
            );

          /*
             IMPORTANT:
             Existing invoice number comes from backend.
          */
          const backendInvoiceNumber =
            getValue(
              invoice,
              "invoiceNumber",
              "InvoiceNumber"
            );

          form.setFieldsValue({
            invoiceNumber:
              backendInvoiceNumber || "",

            invoiceDate: invoiceDate
              ? dayjs(invoiceDate)
              : dayjs(),

            customerId:
              getValue(
                invoice,
                "customerId",
                "CustomerId"
              ) ?? "walk-in",

            warehouseId:
              getValue(
                invoice,
                "warehouseId",
                "WarehouseId"
              ),

            currencyId:
              getValue(
                invoice,
                "currencyId",
                "CurrencyId"
              ),

            exchangeRate:
              safeNumber(
                getValue(
                  invoice,
                  "exchangeRate",
                  "ExchangeRate"
                )
              ) || 1,

            notes:
              getValue(
                invoice,
                "notes",
                "Notes"
              ) || "",
          });

          const mappedItems =
            (
              Array.isArray(
                invoiceItems
              )
                ? invoiceItems
                : []
            ).map(
              (item, index) => {
                const productId =
                  Number(
                    getValue(
                      item,
                      "productId",
                      "ProductId"
                    )
                  );

                const product =
                  products.find(
                    (row) =>
                      getProductId(row) ===
                      productId
                  );

                const quantity =
                  safeNumber(
                    getValue(
                      item,
                      "quantity",
                      "Quantity"
                    )
                  );

                const unitPrice =
                  safeNumber(
                    getValue(
                      item,
                      "unitPrice",
                      "UnitPrice"
                    )
                  );

                const discountAmount =
                  safeNumber(
                    getValue(
                      item,
                      "discountAmount",
                      "DiscountAmount"
                    )
                  );

                const taxAmount =
                  safeNumber(
                    getValue(
                      item,
                      "taxAmount",
                      "TaxAmount"
                    )
                  );

                const totalAmount =
                  getValue(
                    item,
                    "totalAmount",
                    "TotalAmount"
                  );

                return {
                  key:
                    getValue(
                      item,
                      "salesInvoiceItemId",
                      "SalesInvoiceItemId"
                    ) ||
                    `existing-${index}-${productId}`,

                  salesInvoiceItemId:
                    getValue(
                      item,
                      "salesInvoiceItemId",
                      "SalesInvoiceItemId"
                    ),

                  productId,

                  productName:
                    product
                      ? getProductName(
                          product
                        )
                      : `Product #${productId}`,

                  productStockBatchId:
                    getValue(
                      item,
                      "productStockBatchId",
                      "ProductStockBatchId"
                    ),

                  quantity,

                  unitPrice,

                  unitCost:
                    safeNumber(
                      getValue(
                        item,
                        "unitCost",
                        "UnitCost"
                      )
                    ),

                  discountAmount,

                  taxAmount,

                  totalAmount:
                    totalAmount !== null
                      ? safeNumber(
                          totalAmount
                        )
                      : Math.max(
                          0,
                          quantity *
                            unitPrice -
                            discountAmount +
                            taxAmount
                        ),
                };
              }
            );

          setItems(mappedItems);

          setSalesInvoiceIdState(
            Number(id)
          );

          const statusId =
            Number(
              getValue(
                invoice,
                "documentStatusId",
                "DocumentStatusId"
              )
            );

          setIsCompleted(
            statusId === 8 ||
              statusId === 5
          );
        } catch (error) {
          console.error(
            "FAILED TO LOAD SALES INVOICE:",
            error
          );

          console.error(
            "API RESPONSE:",
            error?.response?.data
          );

          messageApi.error(
            error?.response?.data
              ?.message ||
              error?.message ||
              "Failed to load sales invoice."
          );
        } finally {
          setLoading(false);
        }
      },
      [
        form,
        messageApi,
        products,
      ]
    );

  /* =========================================================
     INITIAL LOAD
  ========================================================= */

  useEffect(() => {
    const timer =
      setTimeout(() => {
        void loadMasterData();
      }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [loadMasterData]);

  /* =========================================================
     NEW / EDIT FORM RESET
  ========================================================= */

  useEffect(() => {
    const timer =
      setTimeout(() => {
        if (isEditMode) {
          void loadEditData(
            salesInvoiceId
          );

          return;
        }

        form.resetFields();

        /*
           New Sale:
           Invoice number is NOT generated by frontend.
           Backend will generate it when invoice is created.
        */
        form.setFieldsValue({
          invoiceNumber: undefined,

          invoiceDate:
            dayjs(),

          exchangeRate:
            1,

          currencyId:
            2,

          customerId:
            "walk-in",
        });

        setSalesInvoiceIdState(null);
        setItems([]);
        setStockBatches([]);
        setSelectedProductId(null);
        setSelectedStockBatchId(null);
        setSelectedQuantity(1);
        setSelectedUnitPrice(0);
        setSelectedUnitCost(0);
        setSelectedDiscount(0);
        setSelectedTax(0);
        setIsCompleted(false);
        setPaidAmount(0);
        setPaymentMethodId(1);
      }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [
    form,
    isEditMode,
    loadEditData,
    salesInvoiceId,
  ]);

  /* =========================================================
     PRODUCT OPTIONS
  ========================================================= */

  const productOptions =
    useMemo(
      () =>
        products.map(
          (product) => {
            const productId =
              getProductId(
                product
              );

            const productName =
              getProductName(
                product
              );

            const productCode =
              getProductCode(
                product
              );

            const barcode =
              getProductBarcode(
                product
              );

            return {
              value: productId,

              label:
                `${productCode || "-"} | ` +
                `${productName} | ` +
                `${barcode || "-"}`,

              productCode,
              barcode,
              productName,
            };
          }
        ),
      [products]
    );

  /* =========================================================
     CUSTOMER OPTIONS
  ========================================================= */

  const customerOptions =
    useMemo(
      () => [
        {
          value: "walk-in",
          label:
            "Walk-in Customer",
        },

        ...customers.map(
          (customer) => ({
            value:
              getCustomerId(
                customer
              ),

            label:
              getCustomerName(
                customer
              ),
          })
        ),
      ],
      [customers]
    );

  /* =========================================================
     WAREHOUSE OPTIONS
  ========================================================= */

  const warehouseOptions =
    useMemo(
      () =>
        warehouses.map(
          (warehouse) => ({
            value:
              getWarehouseId(
                warehouse
              ),

            label:
              getWarehouseName(
                warehouse
              ),
          })
        ),
      [warehouses]
    );

  /* =========================================================
     CURRENCY OPTIONS
  ========================================================= */

  const currencyOptions =
    useMemo(
      () =>
        currencies.map(
          (currency) => ({
            value:
              getCurrencyId(
                currency
              ),

            label:
              getCurrencyName(
                currency
              ),
          })
        ),
      [currencies]
    );

  /* =========================================================
     STOCK BATCH OPTIONS
  ========================================================= */

  const stockBatchOptions =
    useMemo(
      () =>
        stockBatches
          .filter(
            (batch) =>
              getBatchRemainingQuantity(
                batch
              ) > 0
          )
          .map(
            (batch) => {
              const batchId =
                getStockBatchId(
                  batch
                );

              const salePrice =
                getBatchSalePrice(
                  batch
                );

              const costPrice =
                getBatchCostPrice(
                  batch
                );

              const remaining =
                getBatchRemainingQuantity(
                  batch
                );

              return {
                value: batchId,

                label:
                  `Sale ${formatMoney(
                    salePrice
                  )} | Cost ${formatMoney(
                    costPrice
                  )} | Stock ${remaining}`,
              };
            }
          ),
      [stockBatches]
    );

  /* =========================================================
     PRODUCT CHANGE
  ========================================================= */

  const handleProductChange =
    useCallback(
      async (productId) => {
        setSelectedProductId(
          productId
        );

        setSelectedStockBatchId(
          null
        );

        setSelectedUnitPrice(0);
        setSelectedUnitCost(0);
        setStockBatches([]);

        if (!productId) {
          return;
        }

        if (!warehouseId) {
          messageApi.warning(
            "Please select a warehouse first."
          );
          return;
        }

        const batches =
          await loadProductStockBatches(
            productId,
            warehouseId
          );

        if (!batches.length) {
          messageApi.warning(
            "No available stock batch found for this product."
          );
          return;
        }

        const availableBatches =
          batches.filter(
            (batch) =>
              getBatchRemainingQuantity(
                batch
              ) > 0
          );

        if (
          !availableBatches.length
        ) {
          messageApi.warning(
            "This product has no remaining stock."
          );
          return;
        }

        const firstBatch =
          availableBatches[0];

        setSelectedStockBatchId(
          getStockBatchId(
            firstBatch
          )
        );

        setSelectedUnitPrice(
          getBatchSalePrice(
            firstBatch
          )
        );

        setSelectedUnitCost(
          getBatchCostPrice(
            firstBatch
          )
        );
      },
      [
        loadProductStockBatches,
        messageApi,
        warehouseId,
      ]
    );

  /* =========================================================
     STOCK BATCH CHANGE
  ========================================================= */

  const handleStockBatchChange =
    useCallback(
      (batchId) => {
        setSelectedStockBatchId(
          batchId
        );

        const selectedBatch =
          stockBatches.find(
            (batch) =>
              getStockBatchId(
                batch
              ) ===
              Number(batchId)
          );

        if (!selectedBatch) {
          setSelectedUnitPrice(0);
          setSelectedUnitCost(0);
          return;
        }

        setSelectedUnitPrice(
          getBatchSalePrice(
            selectedBatch
          )
        );

        setSelectedUnitCost(
          getBatchCostPrice(
            selectedBatch
          )
        );
      },
      [stockBatches]
    );

  /* =========================================================
     ADD ITEM
  ========================================================= */

  const handleAddItem =
    useCallback(() => {
      if (isCompleted) {
        return;
      }

      if (!selectedProductId) {
        messageApi.warning(
          "Please select a product."
        );
        return;
      }

      if (!selectedStockBatchId) {
        messageApi.warning(
          "Please select a stock batch."
        );
        return;
      }

      const quantity =
        safeNumber(
          selectedQuantity
        );

      const unitPrice =
        safeNumber(
          selectedUnitPrice
        );

      const unitCost =
        safeNumber(
          selectedUnitCost
        );

      const discount =
        safeNumber(
          selectedDiscount
        );

      const tax =
        safeNumber(
          selectedTax
        );

      if (quantity <= 0) {
        messageApi.warning(
          "Quantity must be greater than 0."
        );
        return;
      }

      if (unitPrice < 0) {
        messageApi.warning(
          "Sale price cannot be negative."
        );
        return;
      }

      const selectedBatch =
        stockBatches.find(
          (batch) =>
            getStockBatchId(
              batch
            ) ===
            Number(
              selectedStockBatchId
            )
        );

      if (!selectedBatch) {
        messageApi.warning(
          "Selected stock batch was not found."
        );
        return;
      }

      const remainingQuantity =
        getBatchRemainingQuantity(
          selectedBatch
        );

      if (
        quantity >
        remainingQuantity
      ) {
        messageApi.warning(
          `Only ${remainingQuantity} unit(s) available in this batch.`
        );
        return;
      }

      const product =
        products.find(
          (item) =>
            getProductId(item) ===
            Number(
              selectedProductId
            )
        );

      const total =
        quantity * unitPrice -
        discount +
        tax;

      const newItem = {
        key:
          `new-${Date.now()}-${selectedProductId}-${selectedStockBatchId}`,

        salesInvoiceItemId:
          null,

        productId:
          Number(
            selectedProductId
          ),

        productName:
          product
            ? getProductName(product)
            : `Product #${selectedProductId}`,

        productStockBatchId:
          Number(
            selectedStockBatchId
          ),

        quantity,

        unitPrice,

        unitCost,

        discountAmount:
          discount,

        taxAmount:
          tax,

        totalAmount:
          Math.max(
            0,
            total
          ),
      };

      setItems(
        (current) => [
          ...current,
          newItem,
        ]
      );

      setSelectedProductId(null);
      setSelectedStockBatchId(null);
      setSelectedQuantity(1);
      setSelectedUnitPrice(0);
      setSelectedUnitCost(0);
      setStockBatches([]);
      setSelectedDiscount(0);
      setSelectedTax(0);
    }, [
      isCompleted,
      messageApi,
      products,
      selectedDiscount,
      selectedProductId,
      selectedQuantity,
      selectedStockBatchId,
      selectedTax,
      selectedUnitCost,
      selectedUnitPrice,
      stockBatches,
    ]);

  /* =========================================================
     DELETE ITEM
  ========================================================= */

  const handleDeleteItem =
    useCallback(
      async (item) => {
        if (isCompleted) {
          return;
        }

        if (
          item.salesInvoiceItemId
        ) {
          try {
            setLoading(true);

            await deleteSalesInvoiceItem(
              item.salesInvoiceItemId
            );
          } catch (error) {
            console.error(
              "FAILED TO DELETE SALES ITEM:",
              error
            );

            console.error(
              "API RESPONSE:",
              error?.response?.data
            );

            messageApi.error(
              error?.response?.data
                ?.message ||
                error?.message ||
                "Failed to delete sales item."
            );

            return;
          } finally {
            setLoading(false);
          }
        }

        setItems(
          (current) =>
            current.filter(
              (row) =>
                row.key !==
                  item.key &&
                row.salesInvoiceItemId !==
                  item.salesInvoiceItemId
            )
        );
      },
      [
        isCompleted,
        messageApi,
      ]
    );

  /* =========================================================
     COMPLETE SALE
  ========================================================= */

  const handleCompleteSale =
    useCallback(
      async () => {
        try {
          if (isCompleted) {
            messageApi.info(
              "This sale is already completed."
            );
            return;
          }

          const values =
            await form.validateFields();

          if (!items.length) {
            messageApi.warning(
              "Please add at least one product."
            );
            return;
          }

          const finalPaidAmount =
            safeNumber(paidAmount);

          if (
            finalPaidAmount <
            totals.totalAmount
          ) {
            messageApi.warning(
              "Paid amount is less than the invoice total."
            );
            return;
          }

          if (
            !values.warehouseId
          ) {
            messageApi.warning(
              "Please select a warehouse."
            );
            return;
          }

          setLoading(true);

          /*
             IMPORTANT
             ====================================================
             Do NOT send invoiceNumber from frontend.

             Backend generates:

             SV + YY + MM + DD + HH + mm + Sequence

             Example:
             SV260930011701
             ====================================================
          */
          const invoiceData = {
            invoiceDate:
              values.invoiceDate
                ? values.invoiceDate.format(
                    "YYYY-MM-DDTHH:mm:ss.SSSZ"
                  )
                : dayjs().format(
                    "YYYY-MM-DDTHH:mm:ss.SSSZ"
                  ),

            customerId:
              values.customerId ===
                "walk-in" ||
              values.customerId ===
                undefined ||
              values.customerId ===
                null
                ? null
                : Number(
                    values.customerId
                  ),

            warehouseId:
              Number(
                values.warehouseId
              ),

            currencyId:
              Number(
                values.currencyId
              ),

            exchangeRate:
              safeNumber(
                values.exchangeRate
              ) || 1,

            subTotal:
              totals.subTotal,

            discountAmount:
              totals.discountAmount,

            taxAmount:
              totals.taxAmount,

            totalAmount:
              totals.totalAmount,

            notes:
              values.notes || null,

            createdByUserId:
              null,
          };

          let invoiceId =
            salesInvoiceIdState;

          /*
             ====================================================
             CREATE NEW SALES INVOICE
             ====================================================
          */

          if (!invoiceId) {
            const result =
              await createSalesInvoice(
                invoiceData
              );

            /*
               Backend response contains:

               {
                 salesInvoiceId: 1,
                 invoiceNumber: "SV260930011701",
                 ...
               }
            */

            const backendInvoiceNumber =
              getValue(
                result,
                "invoiceNumber",
                "InvoiceNumber"
              );

            /*
               IMPORTANT:
               Put backend-generated invoice number
               into the form immediately.
            */
            if (
              backendInvoiceNumber
            ) {
              form.setFieldsValue({
                invoiceNumber:
                  backendInvoiceNumber,
              });
            }

            invoiceId =
              Number(
                getValue(
                  result,
                  "salesInvoiceId",
                  "SalesInvoiceId"
                )
              );

            if (!invoiceId) {
              throw new Error(
                "Sales Invoice ID was not returned by the server."
              );
            }

            setSalesInvoiceIdState(
              invoiceId
            );
          }

          /* ===================================================
             SAVE SALES ITEMS
          =================================================== */

          for (const item of items) {
            const itemData = {
              salesInvoiceId:
                Number(invoiceId),

              productId:
                Number(
                  item.productId
                ),

              productStockBatchId:
                item.productStockBatchId
                  ? Number(
                      item.productStockBatchId
                    )
                  : null,

              quantity:
                safeNumber(
                  item.quantity
                ),

              unitPrice:
                safeNumber(
                  item.unitPrice
                ),

              unitCost:
                safeNumber(
                  item.unitCost
                ),

              discountAmount:
                safeNumber(
                  item.discountAmount
                ),

              taxAmount:
                safeNumber(
                  item.taxAmount
                ),
            };

            if (
              item.salesInvoiceItemId
            ) {
              await updateSalesInvoiceItem(
                item.salesInvoiceItemId,
                itemData
              );
            } else {
              await createSalesInvoiceItem(
                itemData
              );
            }
          }

          /* ===================================================
             POST SALES INVOICE
          =================================================== */

          const postResult =
            await postSalesInvoice(
              Number(invoiceId)
            );

          /* ===================================================
             CREATE PAYMENT
          =================================================== */

          if (
            finalPaidAmount > 0
          ) {
            try {
              await createSalesPayment({
                salesInvoiceId:
                  Number(invoiceId),

                paymentMethodId:
                  Number(
                    paymentMethodId
                  ),

                amount:
                  finalPaidAmount,

                paymentDate:
                  new Date().toISOString(),

                referenceNumber:
                  null,

                notes:
                  null,
              });
            } catch (
              paymentError
            ) {
              console.error(
                "FAILED TO CREATE SALES PAYMENT:",
                paymentError
              );

              console.error(
                "PAYMENT API RESPONSE:",
                paymentError?.response?.data
              );

              messageApi.warning(
                "Sale completed, but payment could not be saved."
              );
            }
          }

          setSalesInvoiceIdState(
            Number(invoiceId)
          );

          setIsCompleted(true);

          messageApi.success(
            getValue(
              postResult,
              "message",
              "Message"
            ) ||
              "Sale completed successfully."
          );
        } catch (error) {
          console.error(
            "FAILED TO COMPLETE SALE:",
            error
          );

          console.error(
            "API RESPONSE:",
            error?.response?.data
          );

          if (
            error?.errorFields
          ) {
            console.error(
              "FORM VALIDATION ERRORS:",
              error.errorFields
            );

            return;
          }

          messageApi.error(
            error?.response?.data
              ?.message ||
              error?.message ||
              "Failed to complete sale."
          );
        } finally {
          setLoading(false);
        }
      },
      [
        form,
        isCompleted,
        items,
        messageApi,
        paidAmount,
        paymentMethodId,
        salesInvoiceIdState,
        totals,
      ]
    );

  /* =========================================================
     PRINT INVOICE
  ========================================================= */

  const handlePrintInvoice =
    useCallback(() => {
      if (
        !salesInvoiceIdState ||
        !isCompleted
      ) {
        messageApi.warning(
          "Please complete the sale first."
        );
        return;
      }

      /*
         This is the backend-generated number
         already stored in the form.
      */
      const invoiceNumber =
        form.getFieldValue(
          "invoiceNumber"
        ) || "-";

      const invoiceDate =
        form.getFieldValue(
          "invoiceDate"
        );

      const invoiceDateText =
        invoiceDate
          ? dayjs(invoiceDate).format(
              "YYYY-MM-DD HH:mm"
            )
          : dayjs().format(
              "YYYY-MM-DD HH:mm"
            );

      const selectedCustomer =
        customerOptions.find(
          (customer) =>
            customer.value ===
            customerId
        );

      const customerName =
        customerId ===
          "walk-in" ||
        !customerId
          ? "Walk-in Customer"
          : selectedCustomer?.label ||
            "Customer";

      const selectedWarehouse =
        warehouseOptions.find(
          (warehouse) =>
            warehouse.value ===
            warehouseId
        );

      const warehouseName =
        selectedWarehouse?.label ||
        "-";

      const selectedCurrency =
        currencyOptions.find(
          (currency) =>
            currency.value ===
            Number(currencyId)
        );

      const currencyName =
        selectedCurrency?.label ||
        "Currency";

      const itemRows =
        items
          .map(
            (item, index) => `
              <tr>
                <td>${index + 1}</td>

                <td>
                  ${item.productName}
                </td>

                <td class="right">
                  ${safeNumber(
                    item.quantity
                  ).toLocaleString()}
                </td>

                <td class="right">
                  ${formatMoney(
                    item.unitPrice
                  )}
                </td>

                <td class="right">
                  ${formatMoney(
                    item.totalAmount
                  )}
                </td>
              </tr>
            `
          )
          .join("");

      const printWindow =
        window.open(
          "",
          "_blank",
          "width=900,height=700"
        );

      if (!printWindow) {
        messageApi.error(
          "Popup was blocked. Please allow popups for this site."
        );
        return;
      }

      printWindow.document.write(`
        <!DOCTYPE html>

        <html>
          <head>
            <meta charset="UTF-8" />

            <title>
              ${invoiceNumber}
            </title>

            <style>
              * {
                box-sizing: border-box;
              }

              body {
                font-family:
                  Arial,
                  Helvetica,
                  sans-serif;

                margin: 0;
                padding: 30px;

                color: #111;
              }

              .invoice {
                max-width: 800px;
                margin: 0 auto;
              }

              .header {
                text-align: center;
                margin-bottom: 25px;
              }

              .company {
                font-size: 26px;
                font-weight: 700;
              }

              .title {
                font-size: 20px;
                margin-top: 8px;
                font-weight: 600;
              }

              .info {
                display: grid;

                grid-template-columns:
                  1fr 1fr;

                gap: 6px 30px;

                margin-bottom: 20px;
              }

              .info div {
                font-size: 13px;
              }

              table {
                width: 100%;

                border-collapse:
                  collapse;

                margin-top: 15px;
              }

              th,
              td {
                border-bottom:
                  1px solid #ddd;

                padding:
                  8px 6px;

                font-size: 13px;
              }

              th {
                border-top:
                  1px solid #222;

                background:
                  #f5f5f5;

                text-align: left;
              }

              .right {
                text-align: right;
              }

              .totals {
                width: 350px;

                margin-left: auto;

                margin-top: 20px;
              }

              .total-row {
                display: flex;

                justify-content:
                  space-between;

                padding: 5px 0;

                font-size: 14px;
              }

              .grand-total {
                border-top:
                  2px solid #111;

                margin-top: 8px;

                padding-top: 10px;

                font-size: 18px;

                font-weight: 700;
              }

              .payment {
                margin-top: 25px;

                border-top:
                  1px solid #ddd;

                padding-top: 15px;
              }

              .footer {
                text-align: center;

                margin-top: 40px;

                font-size: 12px;

                color: #666;
              }

              @media print {
                body {
                  padding: 0;
                }

                .invoice {
                  max-width: none;
                }
              }
            </style>
          </head>

          <body>
            <div class="invoice">

              <div class="header">

                <div class="company">
                  Infinity POS
                </div>

                <div class="title">
                  SALES INVOICE
                </div>

              </div>

              <div class="info">

                <div>
                  <strong>
                    Invoice No:
                  </strong>

                  ${invoiceNumber}
                </div>

                <div>
                  <strong>
                    Date:
                  </strong>

                  ${invoiceDateText}
                </div>

                <div>
                  <strong>
                    Customer:
                  </strong>

                  ${customerName}
                </div>

                <div>
                  <strong>
                    Warehouse:
                  </strong>

                  ${warehouseName}
                </div>

                <div>
                  <strong>
                    Currency:
                  </strong>

                  ${currencyName}
                </div>

                <div>
                  <strong>
                    Exchange Rate:
                  </strong>

                  ${safeNumber(
                    exchangeRate
                  )}
                </div>

              </div>

              <table>

                <thead>
                  <tr>
                    <th>#</th>

                    <th>
                      Product
                    </th>

                    <th class="right">
                      Qty
                    </th>

                    <th class="right">
                      Unit Price
                    </th>

                    <th class="right">
                      Total
                    </th>
                  </tr>
                </thead>

                <tbody>
                  ${itemRows}
                </tbody>

              </table>

              <div class="totals">

                <div class="total-row">

                  <span>
                    Subtotal
                  </span>

                  <span>
                    ${formatMoney(
                      totals.subTotal
                    )}
                  </span>

                </div>

                <div class="total-row">

                  <span>
                    Discount
                  </span>

                  <span>
                    ${formatMoney(
                      totals.discountAmount
                    )}
                  </span>

                </div>

                <div class="total-row">

                  <span>
                    Tax
                  </span>

                  <span>
                    ${formatMoney(
                      totals.taxAmount
                    )}
                  </span>

                </div>

                <div class="total-row grand-total">

                  <span>
                    Grand Total
                  </span>

                  <span>
                    ${formatMoney(
                      totals.totalAmount
                    )}
                  </span>

                </div>

              </div>

              <div class="payment">

                <div class="total-row">

                  <span>
                    Paid
                  </span>

                  <span>
                    ${formatMoney(
                      paidAmount
                    )}
                  </span>

                </div>

                <div class="total-row">

                  <span>
                    Change
                  </span>

                  <span>
                    ${formatMoney(
                      changeAmount
                    )}
                  </span>

                </div>

                <div class="total-row">

                  <span>
                    Balance
                  </span>

                  <span>
                    ${formatMoney(
                      balanceAmount
                    )}
                  </span>

                </div>

              </div>

              <div class="footer">

                Thank you for your business.

                <br />

                Infinity POS

              </div>

            </div>
          </body>
        </html>
      `);

      printWindow.document.close();

      printWindow.focus();

      setTimeout(() => {
        printWindow.print();
      }, 300);
    }, [
      balanceAmount,
      changeAmount,
      currencyId,
      currencyOptions,
      customerId,
      customerOptions,
      exchangeRate,
      form,
      isCompleted,
      items,
      messageApi,
      paidAmount,
      salesInvoiceIdState,
      totals,
      warehouseId,
      warehouseOptions,
    ]);

  /* =========================================================
     DONE
  ========================================================= */

  const handleDone =
    useCallback(() => {
      if (onSaved) {
        onSaved();
      }
    }, [onSaved]);

  /* =========================================================
     ITEM TABLE
  ========================================================= */

  const itemColumns =
    useMemo(
      () => [
        {
          title: "#",

          width: 50,

          render: (
            _,
            __,
            index
          ) =>
            index + 1,
        },

        {
          title: "Product",

          dataIndex:
            "productName",

          key:
            "productName",
        },

        {
          title: "Batch",

          dataIndex:
            "productStockBatchId",

          key:
            "productStockBatchId",

          width: 90,

          align: "right",

          render: (value) =>
            value || "-",
        },

        {
          title: "Cost",

          dataIndex:
            "unitCost",

          key:
            "unitCost",

          width: 110,

          align: "right",

          render: (value) =>
            formatMoney(value),
        },

        {
          title: "Qty",

          dataIndex:
            "quantity",

          key:
            "quantity",

          width: 90,

          align: "right",

          render: (value) =>
            safeNumber(
              value
            ).toLocaleString(),
        },

        {
          title: "Unit Price",

          dataIndex:
            "unitPrice",

          key:
            "unitPrice",

          width: 120,

          align: "right",

          render: (value) =>
            formatMoney(value),
        },

        {
          title: "Discount",

          dataIndex:
            "discountAmount",

          key:
            "discountAmount",

          width: 110,

          align: "right",

          render: (value) =>
            formatMoney(value),
        },

        {
          title: "Tax",

          dataIndex:
            "taxAmount",

          key:
            "taxAmount",

          width: 100,

          align: "right",

          render: (value) =>
            formatMoney(value),
        },

        {
          title: "Total",

          dataIndex:
            "totalAmount",

          key:
            "totalAmount",

          width: 130,

          align: "right",

          render: (value) =>
            formatMoney(value),
        },

        {
          title: "",

          key: "action",

          width: 60,

          align: "center",

          render: (
            _,
            record
          ) => (
            <Button
              danger
              type="text"
              disabled={
                isCompleted
              }
              icon={
                <DeleteOutlined />
              }
              onClick={() =>
                handleDeleteItem(
                  record
                )
              }
            />
          ),
        },
      ],
      [
        handleDeleteItem,
        isCompleted,
      ]
    );

  /* =========================================================
     RENDER
  ========================================================= */

  return (
    <>
      {contextHolder}

      <Card
        loading={loading}
        styles={{
          body: {
            padding: 20,
          },
        }}
      >
        <Space
          orientation="horizontal"
          style={{
            width: "100%",
            justifyContent:
              "space-between",
            marginBottom: 20,
          }}
        >
          <div>
            <Title
              level={3}
              style={{
                margin: 0,
              }}
            >
              {isEditMode
                ? "Sales Invoice"
                : "New Sale"}
            </Title>

            <Text type="secondary">
              {isCompleted
                ? "Sale Completed"
                : isEditMode
                ? `Invoice ID: ${salesInvoiceId}`
                : "Create and complete sale"}
            </Text>
          </div>

          <Space
            orientation="horizontal"
          >
            <Button
              onClick={onCancel}
              disabled={loading}
            >
              {isCompleted
                ? "Close"
                : "Cancel"}
            </Button>

            {isCompleted && (
              <>
                <Button
                  icon={
                    <PrinterOutlined />
                  }
                  onClick={
                    handlePrintInvoice
                  }
                >
                  Invoice Print
                </Button>

                <Button
                  type="primary"
                  onClick={
                    handleDone
                  }
                >
                  Done
                </Button>
              </>
            )}

            {!isCompleted && (
              <Button
                type="primary"
                icon={
                  <CheckOutlined />
                }
                loading={loading}
                onClick={
                  handleCompleteSale
                }
              >
                Complete Sale
              </Button>
            )}
          </Space>
        </Space>

        <Form
          form={form}
          layout="vertical"
          disabled={isCompleted}
          onKeyDown={(event) => {
            if (
              event.key === "Enter" &&
              event.target?.closest?.(
                ".ant-input-number"
              )
            ) {
              event.preventDefault();
              event.stopPropagation();
            }
          }}
        >
          {/* =================================================
              INVOICE INFORMATION
          ================================================= */}

          <Card
            size="small"
            title="Invoice Information"
          >
            <Row
              gutter={[
                16,
                8,
              ]}
            >
              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Invoice Number"
                  name="invoiceNumber"
                >
                  <Input
                    readOnly
                    placeholder="Auto/System Generated"
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Invoice Date"
                  name="invoiceDate"
                  rules={[
                    {
                      required: true,
                      message:
                        "Invoice date is required.",
                    },
                  ]}
                >
                  <DatePicker
                    showTime
                    format="YYYY-MM-DD HH:mm"
                    style={{
                      width:
                        "100%",
                    }}
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Customer"
                  name="customerId"
                >
                  <Select
                    showSearch
                    allowClear
                    optionFilterProp="label"
                    placeholder="Walk-in Customer"
                    options={
                      customerOptions
                    }
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Warehouse"
                  name="warehouseId"
                  rules={[
                    {
                      required: true,
                      message:
                        "Warehouse is required.",
                    },
                  ]}
                >
                  <Select
                    showSearch
                    optionFilterProp="label"
                    placeholder="Select warehouse"
                    options={
                      warehouseOptions
                    }
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Currency"
                  name="currencyId"
                  rules={[
                    {
                      required: true,
                      message:
                        "Currency is required.",
                    },
                  ]}
                >
                  <Select
                    showSearch
                    optionFilterProp="label"
                    placeholder="Select currency"
                    options={
                      currencyOptions
                    }
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Exchange Rate"
                  name="exchangeRate"
                  rules={[
                    {
                      required: true,
                      message:
                        "Exchange rate is required.",
                    },
                  ]}
                >
                  <InputNumber
                    min={0.000001}
                    step={0.01}
                    precision={6}
                    style={{
                      width:
                        "100%",
                    }}
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={12}
              >
                <Form.Item
                  label="Notes"
                  name="notes"
                >
                  <Input.TextArea
                    rows={1}
                    placeholder="Optional notes"
                  />
                </Form.Item>
              </Col>
            </Row>
          </Card>

          <Divider />

          {/* =================================================
              ADD PRODUCT
          ================================================= */}

          <Card
            size="small"
            title="Add Product"
          >
            <Row
              gutter={[
                12,
                8,
              ]}
            >
              <Col
                xs={24}
                md={7}
              >
                <Text strong>
                  Product
                </Text>

                <Select
                  value={
                    selectedProductId
                  }
                  showSearch
                  allowClear
                  placeholder="Search Product Code / Barcode / Product Name"
                  options={
                    productOptions
                  }
                  onChange={
                    handleProductChange
                  }
                  optionFilterProp="label"
                  filterOption={(
                    input,
                    option
                  ) => {
                    const search =
                      String(
                        input || ""
                      )
                        .trim()
                        .toLowerCase();

                    if (!search) {
                      return true;
                    }

                    const productCode =
                      String(
                        option?.productCode ||
                          ""
                      ).toLowerCase();

                    const barcode =
                      String(
                        option?.barcode ||
                          ""
                      ).toLowerCase();

                    const productName =
                      String(
                        option?.productName ||
                          ""
                      ).toLowerCase();

                    return (
                      productCode.includes(
                        search
                      ) ||
                      barcode.includes(
                        search
                      ) ||
                      productName.includes(
                        search
                      )
                    );
                  }}
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={24}
                md={7}
              >
                <Text strong>
                  Stock Batch / Sale Price
                </Text>

                <Select
                  value={
                    selectedStockBatchId
                  }
                  showSearch
                  allowClear
                  placeholder="Select sale price"
                  optionFilterProp="label"
                  options={
                    stockBatchOptions
                  }
                  onChange={
                    handleStockBatchChange
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={12}
                md={3}
              >
                <Text strong>
                  Quantity
                </Text>

                <InputNumber
                  min={0.000001}
                  step={1}
                  value={
                    selectedQuantity
                  }
                  onChange={(
                    value
                  ) =>
                    setSelectedQuantity(
                      safeNumber(
                        value
                      )
                    )
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={12}
                md={3}
              >
                <Text strong>
                  Sale Price
                </Text>

                <InputNumber
                  min={0}
                  step={1}
                  value={
                    selectedUnitPrice
                  }
                  disabled
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={12}
                md={2}
              >
                <Text strong>
                  Cost
                </Text>

                <InputNumber
                  value={
                    selectedUnitCost
                  }
                  disabled
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={12}
                md={2}
              >
                <Text strong>
                  Discount
                </Text>

                <InputNumber
                  min={0}
                  step={1}
                  value={
                    selectedDiscount
                  }
                  onChange={(
                    value
                  ) =>
                    setSelectedDiscount(
                      safeNumber(
                        value
                      )
                    )
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col
                xs={12}
                md={2}
              >
                <Text strong>
                  Tax
                </Text>

                <InputNumber
                  min={0}
                  step={1}
                  value={
                    selectedTax
                  }
                  onChange={(
                    value
                  ) =>
                    setSelectedTax(
                      safeNumber(
                        value
                      )
                    )
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />
              </Col>

              <Col xs={24}>
                <Button
                  type="dashed"
                  icon={
                    <PlusOutlined />
                  }
                  onClick={
                    handleAddItem
                  }
                >
                  Add Product
                </Button>
              </Col>
            </Row>
          </Card>

          <Divider />

          {/* =================================================
              INVOICE ITEMS
          ================================================= */}

          <Card
            size="small"
            title="Invoice Items"
          >
            <Table
              rowKey={(record) =>
                record.key ||
                record.salesInvoiceItemId
              }
              columns={
                itemColumns
              }
              dataSource={items}
              pagination={
                false
              }
              size="small"
              scroll={{
                x: 1000,
              }}
              locale={{
                emptyText:
                  "No products added.",
              }}
            />
          </Card>

          <Divider />

          {/* =================================================
              PAYMENT + TOTAL
          ================================================= */}

          <Row
            gutter={[
              16,
              16,
            ]}
            justify="end"
            style={{
              marginTop: 20,
            }}
          >
            <Col
              xs={24}
              md={8}
            >
              <Card
                size="small"
                title="Payment"
              >
                <Text strong>
                  Payment Method
                </Text>

                <Select
                  value={
                    paymentMethodId
                  }
                  onChange={
                    setPaymentMethodId
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                    marginBottom: 14,
                  }}
                  options={[
                    {
                      value: 1,
                      label:
                        "Cash",
                    },
                    {
                      value: 2,
                      label:
                        "Bank",
                    },
                    {
                      value: 3,
                      label:
                        "Transfer",
                    },
                  ]}
                />

                <Text strong>
                  Paid Amount
                </Text>

                <InputNumber
                  min={0}
                  step={1}
                  value={
                    paidAmount
                  }
                  onChange={(
                    value
                  ) =>
                    setPaidAmount(
                      safeNumber(
                        value
                      )
                    )
                  }
                  style={{
                    width:
                      "100%",
                    marginTop: 6,
                  }}
                />

                <Divider
                  style={{
                    margin:
                      "14px 0",
                  }}
                />

                <Row
                  justify="space-between"
                >
                  <Text>
                    Total
                  </Text>

                  <Text strong>
                    {formatMoney(
                      totals.totalAmount
                    )}
                  </Text>
                </Row>

                <Row
                  justify="space-between"
                  style={{
                    marginTop: 8,
                  }}
                >
                  <Text>
                    Paid
                  </Text>

                  <Text>
                    {formatMoney(
                      paidAmount
                    )}
                  </Text>
                </Row>

                <Row
                  justify="space-between"
                  style={{
                    marginTop: 8,
                  }}
                >
                  <Text>
                    Balance
                  </Text>

                  <Text>
                    {formatMoney(
                      balanceAmount
                    )}
                  </Text>
                </Row>

                <Row
                  justify="space-between"
                  style={{
                    marginTop: 8,
                  }}
                >
                  <Text strong>
                    Change
                  </Text>

                  <Text strong>
                    {formatMoney(
                      changeAmount
                    )}
                  </Text>
                </Row>
              </Card>
            </Col>

            <Col
              xs={24}
              md={8}
            >
              <Card size="small">
                <Row
                  justify="space-between"
                >
                  <Text>
                    Subtotal
                  </Text>

                  <Text strong>
                    {formatMoney(
                      totals.subTotal
                    )}
                  </Text>
                </Row>

                <Row
                  justify="space-between"
                  style={{
                    marginTop: 8,
                  }}
                >
                  <Text>
                    Discount
                  </Text>

                  <Text>
                    {formatMoney(
                      totals.discountAmount
                    )}
                  </Text>
                </Row>

                <Row
                  justify="space-between"
                  style={{
                    marginTop: 8,
                  }}
                >
                  <Text>
                    Tax
                  </Text>

                  <Text>
                    {formatMoney(
                      totals.taxAmount
                    )}
                  </Text>
                </Row>

                <Divider
                  style={{
                    margin:
                      "12px 0",
                  }}
                />

                <Row
                  justify="space-between"
                >
                  <Text strong>
                    Grand Total
                  </Text>

                  <Title
                    level={4}
                    style={{
                      margin: 0,
                    }}
                  >
                    {formatMoney(
                      totals.totalAmount
                    )}
                  </Title>
                </Row>

                <Text
                  type="secondary"
                  style={{
                    display:
                      "block",
                    marginTop: 6,
                    textAlign:
                      "right",
                  }}
                >
                  Exchange Rate:{" "}
                  {safeNumber(
                    exchangeRate
                  ) || 1}
                </Text>
              </Card>
            </Col>
          </Row>
        </Form>
      </Card>
    </>
  );
}