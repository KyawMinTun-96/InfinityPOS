import {
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
  message,
  Row,
  Select,
  Space,
  Table,
  Typography,
} from "antd";

import {
  DeleteOutlined,
  PlusOutlined,
  SaveOutlined,
} from "@ant-design/icons";

import dayjs from "dayjs";

import {
  getPurchaseInvoice,
  createPurchaseInvoice,
  updatePurchaseInvoice,
  postPurchaseInvoice,
} from "../../api/purchaseInvoicesApi";

import {
  getPurchaseInvoiceItemsByInvoice,
  createPurchaseInvoiceItem,
  updatePurchaseInvoiceItem,
  deletePurchaseInvoiceItem,
} from "../../api/purchaseInvoiceItemsApi";

import { getSuppliers } from "../../api/suppliersApi";
import { getWarehouses } from "../../api/warehousesApi";
import { getCurrencies } from "../../api/currenciesApi";
import { getProducts } from "../../api/productsApi";
import { getDocumentStatuses } from "../../api/documentStatusesApi";

import { getPaymentMethods } from "../../api/paymentMethodsApi";

import {
  getPurchasePaymentsByInvoice,
  createPurchasePayment,
  updatePurchasePayment,
} from "../../api/purchasePaymentsApi";

const { TextArea } = Input;
const { Title, Text } = Typography;

function getValue(obj, camelCaseName, pascalCaseName) {
  return (
    obj?.[camelCaseName] ??
    obj?.[pascalCaseName]
  );
}

function normalizeItem(item, index = 0) {
  return {
    key:
      getValue(
        item,
        "purchaseInvoiceItemId",
        "PurchaseInvoiceItemId"
      ) ??
      `new-${Date.now()}-${index}`,

    purchaseInvoiceItemId:
      getValue(
        item,
        "purchaseInvoiceItemId",
        "PurchaseInvoiceItemId"
      ) ?? null,

    productId: getValue(
      item,
      "productId",
      "ProductId"
    ),

    quantity: Number(
      getValue(
        item,
        "quantity",
        "Quantity"
      ) ?? 0
    ),

    unitCost: Number(
      getValue(
        item,
        "unitCost",
        "UnitCost"
      ) ?? 0
    ),

    salePrice: Number(
      getValue(
        item,
        "salePrice",
        "SalePrice"
      ) ?? 0
    ),

    discountAmount: Number(
      getValue(
        item,
        "discountAmount",
        "DiscountAmount"
      ) ?? 0
    ),

    taxAmount: Number(
      getValue(
        item,
        "taxAmount",
        "TaxAmount"
      ) ?? 0
    ),

    totalAmount: Number(
      getValue(
        item,
        "totalAmount",
        "TotalAmount"
      ) ?? 0
    ),
  };
}

export default function PurchaseInvoiceForm({
  purchaseInvoiceId,
  onCancel,
  onSaved,
}) {
  const [form] = Form.useForm();

  const [messageApi, contextHolder] =
    message.useMessage();

  const [loading, setLoading] =
    useState(false);

  const [initialLoading, setInitialLoading] =
    useState(false);

  const [suppliers, setSuppliers] =
    useState([]);

  const [warehouses, setWarehouses] =
    useState([]);

  const [currencies, setCurrencies] =
    useState([]);

  const [products, setProducts] =
    useState([]);

  const [
    documentStatuses,
    setDocumentStatuses,
  ] = useState([]);

  const [paymentMethods, setPaymentMethods] =
    useState([]);

  const [existingPayment, setExistingPayment] =
    useState(null);

  const [paymentAmountTouched, setPaymentAmountTouched] =
    useState(false);

  const [items, setItems] =
    useState([]);

  const [
    originalItemIds,
    setOriginalItemIds,
  ] = useState([]);

  const [
    currentDocumentStatusId,
    setCurrentDocumentStatusId,
  ] = useState(null);

  const isEditMode =
    Boolean(purchaseInvoiceId);

  const discountAmount =
    Form.useWatch(
      "discountAmount",
      form
    ) ?? 0;

  const taxAmount =
    Form.useWatch(
      "taxAmount",
      form
    ) ?? 0;

  const paymentAmount =
    Form.useWatch(
      "paymentAmount",
      form
    ) ?? 0;

  /* =========================================================
     LOAD PURCHASE DATA
  ========================================================= */

  useEffect(() => {
    let cancelled = false;

    async function loadPurchaseData() {
      try {
        setInitialLoading(true);

        const [
          suppliersData,
          warehousesData,
          currenciesData,
          productsData,
          statusesData,
          paymentMethodsData,
        ] = await Promise.all([
          getSuppliers(true),
          getWarehouses(true),
          getCurrencies(true),
          getProducts(true),
          getDocumentStatuses("PURCHASE"),
          getPaymentMethods(),
        ]);

        if (cancelled) return;

        setSuppliers(suppliersData ?? []);
        setWarehouses(warehousesData ?? []);
        setCurrencies(currenciesData ?? []);
        setProducts(productsData ?? []);
        setDocumentStatuses(statusesData ?? []);
        setPaymentMethods(
          paymentMethodsData ?? []
        );

        /* =====================================================
           NEW PURCHASE
        ===================================================== */

        if (!purchaseInvoiceId) {
          setCurrentDocumentStatusId(null);
          setItems([]);
          setOriginalItemIds([]);
          setExistingPayment(null);
          setPaymentAmountTouched(false);

          form.resetFields();

          const cashMethod =
            (paymentMethodsData ?? []).find(
              (method) =>
                String(
                  getValue(
                    method,
                    "methodCode",
                    "MethodCode"
                  )
                ).toUpperCase() === "CASH"
            );

          form.setFieldsValue({
            invoiceNumber: "",
            invoiceDate: dayjs(),
            exchangeRate: 1,
            discountAmount: 0,
            taxAmount: 0,

            paymentMethodId:
              cashMethod
                ? Number(
                    getValue(
                      cashMethod,
                      "paymentMethodId",
                      "PaymentMethodId"
                    )
                  )
                : undefined,

            paymentAmount: 0,
            paymentReference: "",
          });

          return;
        }

        /* =====================================================
           EDIT PURCHASE
        ===================================================== */

        const [
          invoice,
          invoiceItems,
          loadedPayments,
        ] = await Promise.all([
          getPurchaseInvoice(
            purchaseInvoiceId
          ),

          getPurchaseInvoiceItemsByInvoice(
            purchaseInvoiceId
          ),

          getPurchasePaymentsByInvoice(
            purchaseInvoiceId
          ),
        ]);

        if (cancelled) return;

        const loadedStatusId =
          getValue(
            invoice,
            "documentStatusId",
            "DocumentStatusId"
          );

        setCurrentDocumentStatusId(
          loadedStatusId ?? null
        );

        const normalizedItems =
          (invoiceItems ?? []).map(
            (item, index) =>
              normalizeItem(
                item,
                index
              )
          );

        setItems(normalizedItems);

        setOriginalItemIds(
          normalizedItems
            .map(
              (item) =>
                item.purchaseInvoiceItemId
            )
            .filter(Boolean)
        );

        const firstPayment =
          Array.isArray(loadedPayments) &&
          loadedPayments.length > 0
            ? loadedPayments[0]
            : null;

        setExistingPayment(
          firstPayment
        );

        if (firstPayment) {
          setPaymentAmountTouched(
            true
          );
        } else {
          setPaymentAmountTouched(
            false
          );
        }

        form.setFieldsValue({
          invoiceNumber:
            getValue(
              invoice,
              "invoiceNumber",
              "InvoiceNumber"
            ) ?? "",

          invoiceDate:
            getValue(
              invoice,
              "invoiceDate",
              "InvoiceDate"
            )
              ? dayjs(
                  getValue(
                    invoice,
                    "invoiceDate",
                    "InvoiceDate"
                  )
                )
              : dayjs(),

          supplierId:
            getValue(
              invoice,
              "supplierId",
              "SupplierId"
            ),

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
            Number(
              getValue(
                invoice,
                "exchangeRate",
                "ExchangeRate"
              ) ?? 1
            ),

          discountAmount:
            Number(
              getValue(
                invoice,
                "discountAmount",
                "DiscountAmount"
              ) ?? 0
            ),

          taxAmount:
            Number(
              getValue(
                invoice,
                "taxAmount",
                "TaxAmount"
              ) ?? 0
            ),

          notes:
            getValue(
              invoice,
              "notes",
              "Notes"
            ),

          paymentMethodId:
            firstPayment
              ? Number(
                  getValue(
                    firstPayment,
                    "paymentMethodId",
                    "PaymentMethodId"
                  )
                )
              : undefined,

          paymentAmount:
            firstPayment
              ? Number(
                  getValue(
                    firstPayment,
                    "amount",
                    "Amount"
                  ) ?? 0
                )
              : 0,

          paymentReference:
            firstPayment
              ? getValue(
                  firstPayment,
                  "referenceNumber",
                  "ReferenceNumber"
                ) ?? ""
              : "",
        });
      } catch (error) {
        if (cancelled) return;

        console.error(
          "FAILED TO LOAD PURCHASE INVOICE:",
          error
        );

        messageApi.error(
          error?.response?.data?.message ||
            error?.message ||
            "Failed to load purchase invoice."
        );
      } finally {
        if (!cancelled) {
          setInitialLoading(false);
        }
      }
    }

    loadPurchaseData();

    return () => {
      cancelled = true;
    };
  }, [
    purchaseInvoiceId,
    form,
    messageApi,
  ]);

  /* =========================================================
     AUTO SYNC PAYMENT AMOUNT WITH TOTAL
     
     Only happens when user has not manually changed
     the payment amount.
  ========================================================= */

  const subtotal =
    useMemo(() => {
      return items.reduce(
        (sum, item) => {
          const quantity =
            Number(item.quantity) || 0;

          const unitCost =
            Number(item.unitCost) || 0;

          const discount =
            Number(item.discountAmount) || 0;

          const tax =
            Number(item.taxAmount) || 0;

          return (
            sum +
            quantity * unitCost -
            discount +
            tax
          );
        },
        0
      );
    }, [items]);

  const totalAmount =
    subtotal -
    (Number(discountAmount) || 0) +
    (Number(taxAmount) || 0);

  useEffect(() => {
    if (
      !paymentAmountTouched &&
      !existingPayment
    ) {
      form.setFieldValue(
        "paymentAmount",
        Number(totalAmount)
      );
    }
  }, [
    totalAmount,
    paymentAmountTouched,
    existingPayment,
    form,
  ]);

  /* =========================================================
     OPTIONS
  ========================================================= */

  const productOptions =
    useMemo(() => {
      return (products ?? []).map(
        (product) => ({
          value: getValue(
            product,
            "productId",
            "ProductId"
          ),

          label: `${
            getValue(
              product,
              "sku",
              "SKU"
            ) ?? ""
          } - ${
            getValue(
              product,
              "productName",
              "ProductName"
            ) ?? ""
          }`,
        })
      );
    }, [products]);

  const supplierOptions =
    useMemo(() => {
      return (suppliers ?? []).map(
        (supplier) => ({
          value: getValue(
            supplier,
            "supplierId",
            "SupplierId"
          ),

          label:
            getValue(
              supplier,
              "supplierName",
              "SupplierName"
            ) ??
            getValue(
              supplier,
              "name",
              "Name"
            ) ??
            `Supplier ${getValue(
              supplier,
              "supplierId",
              "SupplierId"
            )}`,
        })
      );
    }, [suppliers]);

  const warehouseOptions =
    useMemo(() => {
      return (warehouses ?? []).map(
        (warehouse) => ({
          value: getValue(
            warehouse,
            "warehouseId",
            "WarehouseId"
          ),

          label:
            getValue(
              warehouse,
              "warehouseName",
              "WarehouseName"
            ) ??
            getValue(
              warehouse,
              "name",
              "Name"
            ) ??
            `Warehouse ${getValue(
              warehouse,
              "warehouseId",
              "WarehouseId"
            )}`,
        })
      );
    }, [warehouses]);

  const currencyOptions =
    useMemo(() => {
      return (currencies ?? []).map(
        (currency) => ({
          value: getValue(
            currency,
            "currencyId",
            "CurrencyId"
          ),

          label:
            getValue(
              currency,
              "currencyName",
              "CurrencyName"
            ) ??
            getValue(
              currency,
              "name",
              "Name"
            ) ??
            getValue(
              currency,
              "currencyCode",
              "CurrencyCode"
            ) ??
            `Currency ${getValue(
              currency,
              "currencyId",
              "CurrencyId"
            )}`,
        })
      );
    }, [currencies]);

  const paymentMethodOptions =
    useMemo(() => {
      return (paymentMethods ?? []).map(
        (method) => ({
          value: Number(
            getValue(
              method,
              "paymentMethodId",
              "PaymentMethodId"
            )
          ),

          label:
            getValue(
              method,
              "methodName",
              "MethodName"
            ) ??
            getValue(
              method,
              "methodCode",
              "MethodCode"
            ) ??
            `Payment Method ${getValue(
              method,
              "paymentMethodId",
              "PaymentMethodId"
            )}`,
        })
      );
    }, [paymentMethods]);

  const draftStatus =
    useMemo(() => {
      return (documentStatuses ?? []).find(
        (status) =>
          getValue(
            status,
            "statusCode",
            "StatusCode"
          ) === "DRAFT"
      );
    }, [documentStatuses]);

  const postedStatus =
    useMemo(() => {
      return (documentStatuses ?? []).find(
        (status) =>
          getValue(
            status,
            "statusCode",
            "StatusCode"
          ) === "POSTED"
      );
    }, [documentStatuses]);

  /* =========================================================
     CURRENT STATUS
  ========================================================= */

  const currentStatus =
    useMemo(() => {
      if (!currentDocumentStatusId) {
        return null;
      }

      return (documentStatuses ?? []).find(
        (status) =>
          Number(
            getValue(
              status,
              "documentStatusId",
              "DocumentStatusId"
            )
          ) ===
          Number(
            currentDocumentStatusId
          )
      );
    }, [
      currentDocumentStatusId,
      documentStatuses,
    ]);

  /* =========================================================
     ITEMS
  ========================================================= */

  function addItem() {
    setItems((prev) => [
      ...prev,

      normalizeItem(
        {
          productId: undefined,
          quantity: 1,
          unitCost: 0,
          salePrice: 0,
          discountAmount: 0,
          taxAmount: 0,
          totalAmount: 0,
        },
        prev.length
      ),
    ]);
  }

  function removeItem(key) {
    setItems((prev) =>
      prev.filter(
        (item) =>
          item.key !== key
      )
    );
  }

  function updateItem(
    key,
    field,
    value
  ) {
    setItems((prev) =>
      prev.map((item) => {
        if (item.key !== key) {
          return item;
        }

        const updated = {
          ...item,
          [field]: value,
        };

        const quantity =
          Number(updated.quantity) || 0;

        const unitCost =
          Number(updated.unitCost) || 0;

        const discount =
          Number(
            updated.discountAmount
          ) || 0;

        const tax =
          Number(
            updated.taxAmount
          ) || 0;

        updated.totalAmount =
          quantity * unitCost -
          discount +
          tax;

        return updated;
      })
    );
  }

  /* =========================================================
     SAVE PAYMENT
  ========================================================= */

  async function savePayment(
    savedInvoiceId,
    values
  ) {
    const selectedPaymentMethodId =
      Number(
        values.paymentMethodId
      );

    const selectedPaymentAmount =
      Number(
        values.paymentAmount
      );

    if (
      !selectedPaymentMethodId
    ) {
      throw new Error(
        "Please select a payment method."
      );
    }

    if (
      !Number.isFinite(
        selectedPaymentAmount
      ) ||
      selectedPaymentAmount <= 0
    ) {
      throw new Error(
        "Payment amount must be greater than 0."
      );
    }

    if (
      selectedPaymentAmount >
        Number(totalAmount) +
          0.000001
    ) {
      throw new Error(
        "Payment amount cannot exceed invoice total."
      );
    }

    const paymentData = {
      purchaseInvoiceId:
        Number(savedInvoiceId),

      paymentMethodId:
        selectedPaymentMethodId,

      amount:
        selectedPaymentAmount,

      paymentDate:
        values.invoiceDate
          ? values.invoiceDate.toISOString()
          : new Date().toISOString(),

      referenceNumber:
        values.paymentReference
          ?.trim() || null,

      notes: null,
    };

    if (existingPayment) {
      await updatePurchasePayment(
        getValue(
          existingPayment,
          "purchasePaymentId",
          "PurchasePaymentId"
        ),
        paymentData
      );
    } else {
      await createPurchasePayment(
        paymentData
      );
    }
  }

  /* =========================================================
     SAVE PURCHASE
  ========================================================= */

  async function handleSave(
    statusId = null
  ) {
    try {
      setLoading(true);

      const values =
        await form.validateFields();

      let finalStatusId = statusId;

      if (
        isEditMode &&
        !finalStatusId
      ) {
        finalStatusId =
          currentDocumentStatusId;
      }

      if (
        !isEditMode &&
        !finalStatusId
      ) {
        finalStatusId =
          getValue(
            draftStatus,
            "documentStatusId",
            "DocumentStatusId"
          );
      }

      if (!finalStatusId) {
        messageApi.error(
          "Purchase document status was not found."
        );

        return;
      }

      if (!items.length) {
        messageApi.error(
          "Please add at least one purchase item."
        );

        return;
      }

      for (
        let index = 0;
        index < items.length;
        index++
      ) {
        const item = items[index];

        if (!item.productId) {
          messageApi.error(
            `Please select a product for item ${
              index + 1
            }.`
          );

          return;
        }

        if (
          Number(item.quantity) <= 0
        ) {
          messageApi.error(
            `Quantity must be greater than 0 for item ${
              index + 1
            }.`
          );

          return;
        }

        if (
          Number(item.unitCost) < 0
        ) {
          messageApi.error(
            `Unit cost cannot be negative for item ${
              index + 1
            }.`
          );

          return;
        }

        if (
          Number(item.salePrice) < 0
        ) {
          messageApi.error(
            `Sale price cannot be negative for item ${
              index + 1
            }.`
          );

          return;
        }
      }

      /* =====================================================
         PAYMENT VALIDATION
      ===================================================== */

      const selectedPaymentMethodId =
        Number(
          values.paymentMethodId
        );

      const selectedPaymentAmount =
        Number(
          values.paymentAmount
        );

      if (
        !selectedPaymentMethodId
      ) {
        messageApi.error(
          "Please select a payment method."
        );

        return;
      }

      if (
        !Number.isFinite(
          selectedPaymentAmount
        ) ||
        selectedPaymentAmount <= 0
      ) {
        messageApi.error(
          "Payment amount must be greater than 0."
        );

        return;
      }

      if (
        selectedPaymentAmount >
          Number(totalAmount) +
            0.000001
      ) {
        messageApi.error(
          "Payment amount cannot exceed invoice total."
        );

        return;
      }

      /* =====================================================
         INVOICE DATA
      ===================================================== */

      const invoiceData = {
        invoiceNumber:
          isEditMode
            ? values.invoiceNumber
            : null,

            invoiceDate:
              values.invoiceDate
                ? values.invoiceDate.format(
                    "YYYY-MM-DDT00:00:00"
                  )
                : dayjs().format(
                    "YYYY-MM-DDT00:00:00"
                  ),

        supplierId:
          values.supplierId ?? null,

        warehouseId:
          values.warehouseId,

        currencyId:
          values.currencyId,

        exchangeRate:
          Number(
            values.exchangeRate ?? 1
          ),

        subTotal:
          Number(subtotal),

        discountAmount:
          Number(
            values.discountAmount ?? 0
          ),

        taxAmount:
          Number(
            values.taxAmount ?? 0
          ),

        totalAmount:
          Number(totalAmount),

        documentStatusId:
          Number(finalStatusId),

        notes:
          values.notes ?? null,
      };

      /* =====================================================
         SAVE INVOICE
      ===================================================== */

      let savedInvoice;

      if (isEditMode) {
        savedInvoice =
          await updatePurchaseInvoice(
            purchaseInvoiceId,
            invoiceData
          );
      } else {
        savedInvoice =
          await createPurchaseInvoice(
            invoiceData
          );
      }

      const savedInvoiceId =
        isEditMode
          ? purchaseInvoiceId
          : getValue(
              savedInvoice,
              "purchaseInvoiceId",
              "PurchaseInvoiceId"
            );

      if (!savedInvoiceId) {
        throw new Error(
          "Purchase invoice ID was not returned."
        );
      }

      /* =====================================================
         GENERATED VOUCHER NO
      ===================================================== */

      const generatedVoucherNo =
        getValue(
          savedInvoice,
          "invoiceNumber",
          "InvoiceNumber"
        );

      if (
        !isEditMode &&
        generatedVoucherNo
      ) {
        form.setFieldsValue({
          invoiceNumber:
            generatedVoucherNo,
        });
      }

      /* =====================================================
         SAVE ITEMS
      ===================================================== */

      const existingItemIds =
        items
          .map(
            (item) =>
              item.purchaseInvoiceItemId
          )
          .filter(Boolean);

      for (const item of items) {
        const itemData = {
          purchaseInvoiceId:
            Number(savedInvoiceId),

          productId:
            Number(item.productId),

          quantity:
            Number(item.quantity),

          unitCost:
            Number(item.unitCost),

          salePrice:
            Number(
              item.salePrice ?? 0
            ),

          discountAmount:
            Number(
              item.discountAmount ?? 0
            ),

          taxAmount:
            Number(
              item.taxAmount ?? 0
            ),

          totalAmount:
            Number(
              item.totalAmount ?? 0
            ),
        };

        if (
          item.purchaseInvoiceItemId
        ) {
          await updatePurchaseInvoiceItem(
            item.purchaseInvoiceItemId,
            itemData
          );
        } else {
          await createPurchaseInvoiceItem(
            itemData
          );
        }
      }

      /* =====================================================
         DELETE REMOVED ITEMS
      ===================================================== */

      if (isEditMode) {
        const removedItemIds =
          originalItemIds.filter(
            (originalId) =>
              !existingItemIds.includes(
                originalId
              )
          );

        for (
          const itemId of
            removedItemIds
        ) {
          await deletePurchaseInvoiceItem(
            itemId
          );
        }
      }

      /* =====================================================
         SAVE PAYMENT
      ===================================================== */

      await savePayment(
        savedInvoiceId,
        values
      );

      /* =====================================================
         UPDATE LOCAL PAYMENT STATE
      ===================================================== */

      setExistingPayment({
        purchaseInvoiceId:
          Number(savedInvoiceId),

        paymentMethodId:
          selectedPaymentMethodId,

        amount:
          selectedPaymentAmount,

        paymentDate:
          values.invoiceDate
            ? values.invoiceDate.format(
                "YYYY-MM-DDT00:00:00"
              )
            : dayjs().format(
                "YYYY-MM-DDT00:00:00"
              ),

        referenceNumber:
          values.paymentReference
            ?.trim() || null,
      });

      setPaymentAmountTouched(
        true
      );

      setCurrentDocumentStatusId(
        Number(finalStatusId)
      );

      messageApi.success(
        isEditMode
          ? "Purchase invoice updated successfully."
          : generatedVoucherNo
            ? `Purchase invoice ${generatedVoucherNo} created successfully.`
            : "Purchase invoice created successfully."
      );

      if (onSaved) {
        onSaved();
      }
    } catch (error) {
      console.error(
        "FAILED TO SAVE PURCHASE:",
        error
      );

      if (error?.errorFields) {
        console.error(
          "FORM VALIDATION ERRORS:",
          error.errorFields
        );
      }

      console.error(
        "API RESPONSE:",
        error?.response?.data
      );

      messageApi.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to save purchase."
      );
    } finally {
      setLoading(false);
    }
  }

  /* =========================================================
     POST PURCHASE
  ========================================================= */

  async function handlePostPurchase() {
    if (
      !isEditMode ||
      !purchaseInvoiceId
    ) {
      messageApi.error(
        "Please save the purchase invoice first."
      );

      return;
    }

    const draftId =
      Number(
        getValue(
          draftStatus,
          "documentStatusId",
          "DocumentStatusId"
        )
      );

    const currentStatusId =
      Number(
        currentDocumentStatusId
      );

    if (
      !draftId ||
      currentStatusId !== draftId
    ) {
      messageApi.error(
        "Only DRAFT purchase invoices can be posted."
      );

      return;
    }

    if (!items.length) {
      messageApi.error(
        "Cannot post purchase invoice without items."
      );

      return;
    }

    const values =
      form.getFieldsValue();

    const selectedPaymentMethodId =
      Number(
        values.paymentMethodId
      );

    const selectedPaymentAmount =
      Number(
        values.paymentAmount
      );

    if (
      !selectedPaymentMethodId
    ) {
      messageApi.error(
        "Please select a payment method before posting."
      );

      return;
    }

    if (
      !Number.isFinite(
        selectedPaymentAmount
      ) ||
      selectedPaymentAmount <= 0
    ) {
      messageApi.error(
        "Payment amount must be greater than 0 before posting."
      );

      return;
    }

    if (
      selectedPaymentAmount >
        Number(totalAmount) +
          0.000001
    ) {
      messageApi.error(
        "Payment amount cannot exceed invoice total."
      );

      return;
    }

    try {
      setLoading(true);

      /* =====================================================
         MAKE SURE PAYMENT IS SAVED BEFORE POSTING
      ===================================================== */

      await savePayment(
        purchaseInvoiceId,
        values
      );

      const result =
        await postPurchaseInvoice(
          purchaseInvoiceId
        );

      const postedId =
        Number(
          getValue(
            result,
            "documentStatusId",
            "DocumentStatusId"
          )
        );

      setCurrentDocumentStatusId(
        postedId ||
          Number(
            getValue(
              postedStatus,
              "documentStatusId",
              "DocumentStatusId"
            )
          )
      );

      messageApi.success(
        result?.message ||
          "Purchase invoice posted successfully."
      );

      if (onSaved) {
        onSaved();
      }
    } catch (error) {
      console.error(
        "FAILED TO POST PURCHASE:",
        error
      );

      console.error(
        "API RESPONSE:",
        error?.response?.data
      );

      messageApi.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to post purchase invoice."
      );
    } finally {
      setLoading(false);
    }
  }

  /* =========================================================
     ITEM COLUMNS
  ========================================================= */

  const itemColumns = [
    {
      title: "#",
      width: 55,
      align: "center",

      render: (
        _,
        __,
        index
      ) =>
        index + 1,
    },

    {
      title: "Product",
      dataIndex: "productId",
      width: 320,

      render: (
        value,
        record
      ) => (
        <Select
          value={value}
          showSearch
          optionFilterProp="label"
          options={
            productOptions
          }
          placeholder="Select product"
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "productId",
              newValue
            )
          }
        />
      ),
    },

    {
      title: "Qty",
      dataIndex: "quantity",
      width: 120,

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={4}
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "quantity",
              newValue ?? 0
            )
          }
        />
      ),
    },

    {
      title: "Unit Cost",
      dataIndex: "unitCost",
      width: 150,

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={4}
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "unitCost",
              newValue ?? 0
            )
          }
        />
      ),
    },

    {
      title: "Sale Price",
      dataIndex: "salePrice",
      width: 150,

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={4}
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "salePrice",
              newValue ?? 0
            )
          }
        />
      ),
    },

    {
      title: "Discount",
      dataIndex: "discountAmount",
      width: 130,

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={4}
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "discountAmount",
              newValue ?? 0
            )
          }
        />
      ),
    },

    {
      title: "Tax",
      dataIndex: "taxAmount",
      width: 130,

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={4}
          style={{
            width: "100%",
          }}
          onChange={(newValue) =>
            updateItem(
              record.key,
              "taxAmount",
              newValue ?? 0
            )
          }
        />
      ),
    },

    {
      title: "Total",
      dataIndex: "totalAmount",
      width: 150,
      align: "right",

      render: (value) =>
        Number(
          value ?? 0
        ).toLocaleString(
          undefined,
          {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          }
        ),
    },

    {
      title: "Action",
      width: 80,
      align: "center",

      render: (
        _,
        record
      ) => (
        <Button
          danger
          type="text"
          icon={
            <DeleteOutlined />
          }
          onClick={() =>
            removeItem(
              record.key
            )
          }
        />
      ),
    },
  ];

  /* =========================================================
     INITIAL LOADING
  ========================================================= */

  if (initialLoading) {
    return (
      <>
        {contextHolder}

        <Card loading>
          <Title level={4}>
            Purchase Invoice
          </Title>
        </Card>
      </>
    );
  }

  /* =========================================================
     RENDER
  ========================================================= */

  return (
    <>
      {contextHolder}

      <Card>
        <Space
          orientation="horizontal"
          style={{
            width: "100%",
            justifyContent:
              "space-between",
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
                ? "Edit Purchase Invoice"
                : "New Purchase Invoice"}
            </Title>

            {isEditMode &&
              currentStatus && (
                <Text type="secondary">
                  Status:{" "}
                  {getValue(
                    currentStatus,
                    "statusName",
                    "StatusName"
                  )}
                </Text>
              )}
          </div>

          <Space orientation="horizontal">
            {isEditMode && (
              <Button
                disabled
                type="default"
              >
                {getValue(
                  currentStatus,
                  "statusName",
                  "StatusName"
                ) ??
                  "Unknown Status"}
              </Button>
            )}
          </Space>
        </Space>

        <Divider />

        <Form
          form={form}
          layout="vertical"
          initialValues={{
            invoiceNumber: "",
            invoiceDate: dayjs(),
            exchangeRate: 1,
            discountAmount: 0,
            taxAmount: 0,
            paymentAmount: 0,
            paymentReference: "",
          }}
          onFinish={() =>
            handleSave()
          }
          onKeyDown={(e) => {
            if (
              e.key === "Enter" &&
              e.target?.closest?.(
                ".ant-input-number"
              )
            ) {
              e.preventDefault();
              e.stopPropagation();
            }
          }}
        >
          {/* =================================================
              HEADER
          ================================================= */}

          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item
                label="Voucher No"
                name="invoiceNumber"
              >
                <Input
                  placeholder={
                    isEditMode
                      ? "Voucher No"
                      : "Auto generated after save"
                  }
                  disabled
                />
              </Form.Item>
            </Col>

            <Col xs={24} md={8}>
              <Form.Item
                label="Invoice Date"
                name="invoiceDate"
                rules={[
                  {
                    required: true,
                    message:
                      "Please select invoice date.",
                  },
                ]}
              >
                <DatePicker
                  style={{
                    width: "100%",
                  }}
                  format="YYYY-MM-DD"
                />
              </Form.Item>
            </Col>

            <Col xs={24} md={8}>
              <Form.Item
                label="Supplier"
                name="supplierId"
              >
                <Select
                  allowClear
                  showSearch
                  optionFilterProp="label"
                  placeholder="Select supplier"
                  options={
                    supplierOptions
                  }
                />
              </Form.Item>
            </Col>
          </Row>

          {/* =================================================
              WAREHOUSE / CURRENCY
          ================================================= */}

          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item
                label="Warehouse"
                name="warehouseId"
                rules={[
                  {
                    required: true,
                    message:
                      "Please select warehouse.",
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

            <Col xs={24} md={8}>
              <Form.Item
                label="Currency"
                name="currencyId"
                rules={[
                  {
                    required: true,
                    message:
                      "Please select currency.",
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

            <Col xs={24} md={8}>
              <Form.Item
                label="Exchange Rate"
                name="exchangeRate"
                rules={[
                  {
                    required: true,
                    message:
                      "Please enter exchange rate.",
                  },
                ]}
              >
                <InputNumber
                  min={0.000001}
                  precision={6}
                  style={{
                    width: "100%",
                  }}
                />
              </Form.Item>
            </Col>
          </Row>

          <Divider />

          {/* =================================================
              PURCHASE ITEMS
          ================================================= */}

          <Space
            orientation="horizontal"
            style={{
              width: "100%",
              justifyContent:
                "space-between",
              marginBottom: 16,
            }}
          >
            <Title
              level={4}
              style={{
                margin: 0,
              }}
            >
              Purchase Items
            </Title>

            <Button
              type="dashed"
              icon={
                <PlusOutlined />
              }
              onClick={addItem}
            >
              Add Item
            </Button>
          </Space>

          <Table
            rowKey="key"
            columns={itemColumns}
            dataSource={items}
            pagination={false}
            scroll={{
              x: 1350,
            }}
          />

          <Divider />

          {/* =================================================
              NOTES / TOTAL / PAYMENT
          ================================================= */}

          <Row gutter={24}>
            <Col xs={24} md={12}>
              <Form.Item
                label="Notes"
                name="notes"
              >
                <TextArea
                  rows={5}
                  placeholder="Notes"
                />
              </Form.Item>
            </Col>

            <Col xs={24} md={12}>
              <Card
                size="small"
                style={{
                  background:
                    "#fafafa",
                }}
              >
                <Row
                  justify="space-between"
                  style={{
                    marginBottom: 12,
                  }}
                >
                  <Text>
                    Subtotal
                  </Text>

                  <Text strong>
                    {subtotal.toLocaleString(
                      undefined,
                      {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      }
                    )}
                  </Text>
                </Row>

                <Form.Item
                  label="Invoice Discount"
                  name="discountAmount"
                  style={{
                    marginBottom: 12,
                  }}
                >
                  <InputNumber
                    min={0}
                    precision={4}
                    style={{
                      width: "100%",
                    }}
                  />
                </Form.Item>

                <Form.Item
                  label="Invoice Tax"
                  name="taxAmount"
                  style={{
                    marginBottom: 12,
                  }}
                >
                  <InputNumber
                    min={0}
                    precision={4}
                    style={{
                      width: "100%",
                    }}
                  />
                </Form.Item>

                <Divider />

                <Row
                  justify="space-between"
                >
                  <Text strong>
                    Total Amount
                  </Text>

                  <Title
                    level={4}
                    style={{
                      margin: 0,
                    }}
                  >
                    {totalAmount.toLocaleString(
                      undefined,
                      {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      }
                    )}
                  </Title>
                </Row>
              </Card>
            </Col>
          </Row>

          <Divider />

          {/* =================================================
              PAYMENT
          ================================================= */}

          <Card
            size="small"
            title="Payment"
            style={{
              marginBottom: 24,
            }}
          >
            <Row gutter={16}>
              <Col xs={24} md={8}>
                <Form.Item
                  label="Payment Method"
                  name="paymentMethodId"
                  rules={[
                    {
                      required: true,
                      message:
                        "Please select payment method.",
                    },
                  ]}
                >
                  <Select
                    showSearch
                    optionFilterProp="label"
                    placeholder="Select payment method"
                    options={
                      paymentMethodOptions
                    }
                  />
                </Form.Item>
              </Col>

              <Col xs={24} md={8}>
                <Form.Item
                  label="Paid Amount"
                  name="paymentAmount"
                  rules={[
                    {
                      required: true,
                      message:
                        "Please enter paid amount.",
                    },
                    {
                      validator:
                        async (
                          _,
                          value
                        ) => {
                          const amount =
                            Number(
                              value
                            );

                          if (
                            !Number.isFinite(
                              amount
                            ) ||
                            amount <= 0
                          ) {
                            throw new Error(
                              "Paid amount must be greater than 0."
                            );
                          }

                          if (
                            amount >
                            Number(
                              totalAmount
                            ) +
                              0.000001
                          ) {
                            throw new Error(
                              "Paid amount cannot exceed invoice total."
                            );
                          }
                        },
                    },
                  ]}
                >
                  <InputNumber
                    min={0}
                    precision={4}
                    style={{
                      width: "100%",
                    }}
                    onChange={() =>
                      setPaymentAmountTouched(
                        true
                      )
                    }
                  />
                </Form.Item>
              </Col>

              <Col xs={24} md={8}>
                <Form.Item
                  label="Reference Number"
                  name="paymentReference"
                >
                  <Input
                    placeholder="Reference / cheque / transaction no."
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row
              justify="end"
            >
              <Text type="secondary">
                Payment:{" "}
                {Number(
                  paymentAmount
                ).toLocaleString(
                  undefined,
                  {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2,
                  }
                )}
              </Text>
            </Row>
          </Card>

          <Divider />

          {/* =================================================
              BUTTONS
          ================================================= */}

          <Space
            orientation="horizontal"
            style={{
              width: "100%",
              justifyContent:
                "flex-end",
            }}
          >
            <Button
              onClick={onCancel}
              disabled={loading}
            >
              Cancel
            </Button>

            {!isEditMode && (
              <Button
                type="default"
                icon={
                  <SaveOutlined />
                }
                loading={loading}
                onClick={() => {
                  const draftId =
                    getValue(
                      draftStatus,
                      "documentStatusId",
                      "DocumentStatusId"
                    );

                  handleSave(
                    draftId
                  );
                }}
              >
                Save Draft
              </Button>
            )}

            {isEditMode && (
              <Button
                type="primary"
                icon={
                  <SaveOutlined />
                }
                loading={loading}
                onClick={() =>
                  handleSave()
                }
              >
                Update
              </Button>
            )}

            {isEditMode &&
              Number(
                currentDocumentStatusId
              ) ===
                Number(
                  getValue(
                    draftStatus,
                    "documentStatusId",
                    "DocumentStatusId"
                  )
                ) && (
                <Button
                  type="primary"
                  loading={loading}
                  onClick={
                    handlePostPurchase
                  }
                >
                  Post / Receive
                </Button>
              )}
          </Space>
        </Form>
      </Card>
    </>
  );
}