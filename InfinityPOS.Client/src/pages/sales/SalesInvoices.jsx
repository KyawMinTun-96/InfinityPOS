import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Col,
  DatePicker,
  Divider,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  message,
} from "antd";
import {
  PlusOutlined,
  ReloadOutlined,
  LeftOutlined,
  RightOutlined,
  PrinterOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";

import {
  getSalesInvoices,
  getSalesInvoice,
} from "../../api/salesInvoicesApi";

import {
  getSalesInvoiceItemsByInvoice,
} from "../../api/salesInvoiceItemsApi";

import {
  getSalesPaymentsByInvoice,
} from "../../api/salesPaymentsApi";

import { getProducts } from "../../api/productsApi";
import { getCustomers } from "../../api/customersApi";
import { getWarehouses } from "../../api/warehousesApi";
import { getCurrencies } from "../../api/currenciesApi";
import { getDocumentStatuses } from "../../api/documentStatusesApi";

const { RangePicker } = DatePicker;

const STATUS_CODES = {
  DRAFT: "DRAFT",
  POSTED: "POSTED",
  VOID: "VOID",
};

function getValue(obj, ...keys) {
  for (const key of keys) {
    if (
      obj?.[key] !== undefined &&
      obj?.[key] !== null
    ) {
      return obj[key];
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

function escapeHtml(value) {
  return String(value ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

function getTodayRange() {
  const today = dayjs();

  return [
    today.startOf("day"),
    today.endOf("day"),
  ];
}

function getStatusColor(statusCode) {
  if (statusCode === STATUS_CODES.POSTED) {
    return "green";
  }

  if (statusCode === STATUS_CODES.VOID) {
    return "red";
  }

  return "gold";
}

function getShortWarehouseName(name) {
  if (!name) {
    return "-";
  }

  return String(name)
    .replace(/ Warehouse$/i, "")
    .replace(/^Main$/i, "Main")
    .trim();
}

function getProductName(product) {
  return (
    getValue(
      product,
      "productName",
      "ProductName",
      "name",
      "Name"
    ) || "-"
  );
}

export default function SalesInvoices({
  onNew,
  onEdit,
}) {
  const [messageApi, contextHolder] =
    message.useMessage();

  const [invoices, setInvoices] = useState([]);
  const [products, setProducts] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [warehouses, setWarehouses] = useState([]);
  const [currencies, setCurrencies] = useState([]);
  const [statuses, setStatuses] = useState([]);

  const [loading, setLoading] =
    useState(false);

  const [searchText, setSearchText] =
    useState("");

  const [customerId, setCustomerId] =
    useState("all");

  const [warehouseId, setWarehouseId] =
    useState("all");

  const [statusId, setStatusId] =
    useState("all");

  const [dateFilter, setDateFilter] =
    useState("today");

  const [dateRange, setDateRange] =
    useState(getTodayRange());

  const loadData = useCallback(
    async () => {
      try {
        setLoading(true);

        const [
          invoiceData,
          productData,
          customerData,
          warehouseData,
          currencyData,
          statusData,
        ] = await Promise.all([
          getSalesInvoices(),
          getProducts(true),
          getCustomers(true),
          getWarehouses(true),
          getCurrencies(true),
          getDocumentStatuses("SALES"),
        ]);

        setInvoices(
          Array.isArray(invoiceData)
            ? invoiceData
            : []
        );

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

        setStatuses(
          Array.isArray(statusData)
            ? statusData
            : []
        );
      } catch (error) {
        console.error(
          "FAILED TO LOAD SALES INVOICES:",
          error
        );

        console.error(
          "API RESPONSE:",
          error?.response?.data
        );

        messageApi.error(
          error?.response?.data?.message ||
            error?.message ||
            "Failed to load sales invoices."
        );
      } finally {
        setLoading(false);
      }
    },
    [messageApi]
  );

  useEffect(() => {
    const timer = setTimeout(() => {
      void loadData();
    }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [loadData]);

  const productMap = useMemo(() => {
    const map = new Map();

    products.forEach((product) => {
      const id = safeNumber(
        getValue(
          product,
          "productId",
          "ProductId"
        )
      );

      map.set(id, {
        name: getProductName(product),
        sku:
          getValue(
            product,
            "sku",
            "SKU"
          ) || "-",
        barcode:
          getValue(
            product,
            "barcode",
            "Barcode"
          ) || "-",
      });
    });

    return map;
  }, [products]);

  const customerMap = useMemo(() => {
    const map = new Map();

    customers.forEach((customer) => {
      const id = safeNumber(
        getValue(
          customer,
          "customerId",
          "CustomerId"
        )
      );

      const name =
        getValue(
          customer,
          "customerName",
          "CustomerName",
          "name",
          "Name"
        ) || "-";

      map.set(id, name);
    });

    return map;
  }, [customers]);

  const warehouseMap = useMemo(() => {
    const map = new Map();

    warehouses.forEach((warehouse) => {
      const id = safeNumber(
        getValue(
          warehouse,
          "warehouseId",
          "WarehouseId"
        )
      );

      const name =
        getValue(
          warehouse,
          "warehouseName",
          "WarehouseName",
          "name",
          "Name"
        ) || "-";

      map.set(id, name);
    });

    return map;
  }, [warehouses]);

  const currencyMap = useMemo(() => {
    const map = new Map();

    currencies.forEach((currency) => {
      const id = safeNumber(
        getValue(
          currency,
          "currencyId",
          "CurrencyId"
        )
      );

      const name =
        getValue(
          currency,
          "currencyCode",
          "CurrencyCode",
          "code",
          "Code",
          "currencyName",
          "CurrencyName",
          "name",
          "Name"
        ) || "-";

      map.set(id, name);
    });

    return map;
  }, [currencies]);

  const statusMap = useMemo(() => {
    const map = new Map();

    statuses.forEach((status) => {
      const id = safeNumber(
        getValue(
          status,
          "documentStatusId",
          "DocumentStatusId"
        )
      );

      const code =
        getValue(
          status,
          "statusCode",
          "StatusCode"
        ) || "";

      const name =
        getValue(
          status,
          "statusName",
          "StatusName"
        ) ||
        code ||
        "-";

      map.set(id, {
        code,
        name,
      });
    });

    return map;
  }, [statuses]);

  const getInvoiceStatus = useCallback(
    (invoice) => {
      const id = safeNumber(
        getValue(
          invoice,
          "documentStatusId",
          "DocumentStatusId"
        )
      );

      const mapped = statusMap.get(id);

      return {
        code:
          mapped?.code ||
          getValue(
            invoice,
            "statusCode",
            "StatusCode"
          ) ||
          "",

        name:
          mapped?.name ||
          getValue(
            invoice,
            "statusName",
            "StatusName"
          ) ||
          "-",
      };
    },
    [statusMap]
  );

  const handleDateFilterChange = (
    value
  ) => {
    const today = dayjs();

    setDateFilter(value);

    if (value === "all") {
      setDateRange(null);
      return;
    }

    if (value === "today") {
      setDateRange([
        today.startOf("day"),
        today.endOf("day"),
      ]);
      return;
    }

    if (value === "yesterday") {
      const yesterday =
        today.subtract(1, "day");

      setDateRange([
        yesterday.startOf("day"),
        yesterday.endOf("day"),
      ]);
      return;
    }

    if (value === "thisWeek") {
      setDateRange([
        today.startOf("week"),
        today.endOf("week"),
      ]);
      return;
    }

    if (value === "thisMonth") {
      setDateRange([
        today.startOf("month"),
        today.endOf("month"),
      ]);
      return;
    }

    if (value === "thisYear") {
      setDateRange([
        today.startOf("year"),
        today.endOf("year"),
      ]);
    }
  };

  const handleRangeChange = (
    values
  ) => {
    if (
      !values ||
      values.length !== 2
    ) {
      setDateRange(null);
      setDateFilter("all");
      return;
    }

    setDateRange([
      values[0].startOf("day"),
      values[1].endOf("day"),
    ]);

    setDateFilter("custom");
  };

  const handlePreviousDay = () => {
    const baseDate =
      dateRange?.[0] || dayjs();

    const previousDay =
      baseDate.subtract(1, "day");

    setDateRange([
      previousDay.startOf("day"),
      previousDay.endOf("day"),
    ]);

    setDateFilter("custom");
  };

  const handleNextDay = () => {
    const baseDate =
      dateRange?.[0] || dayjs();

    const nextDay =
      baseDate.add(1, "day");

    setDateRange([
      nextDay.startOf("day"),
      nextDay.endOf("day"),
    ]);

    setDateFilter("custom");
  };

  const filteredInvoices = useMemo(() => {
    const search =
      searchText.trim().toLowerCase();

    return invoices
      .filter((invoice) => {
        const invoiceDate = dayjs(
          getValue(
            invoice,
            "invoiceDate",
            "InvoiceDate"
          )
        );

        if (
          dateRange &&
          dateRange.length === 2 &&
          (
            invoiceDate.isBefore(
              dateRange[0]
            ) ||
            invoiceDate.isAfter(
              dateRange[1]
            )
          )
        ) {
          return false;
        }

        const invoiceCustomerId =
          safeNumber(
            getValue(
              invoice,
              "customerId",
              "CustomerId"
            )
          );

        if (
          customerId !== "all" &&
          invoiceCustomerId !==
            safeNumber(customerId)
        ) {
          return false;
        }

        const invoiceWarehouseId =
          safeNumber(
            getValue(
              invoice,
              "warehouseId",
              "WarehouseId"
            )
          );

        if (
          warehouseId !== "all" &&
          invoiceWarehouseId !==
            safeNumber(warehouseId)
        ) {
          return false;
        }

        const invoiceStatusId =
          safeNumber(
            getValue(
              invoice,
              "documentStatusId",
              "DocumentStatusId"
            )
          );

        if (
          statusId !== "all" &&
          invoiceStatusId !==
            safeNumber(statusId)
        ) {
          return false;
        }

        if (!search) {
          return true;
        }

        const invoiceNumber =
          String(
            getValue(
              invoice,
              "invoiceNumber",
              "InvoiceNumber"
            ) || ""
          ).toLowerCase();

        const customerName =
          String(
            customerMap.get(
              invoiceCustomerId
            ) || ""
          ).toLowerCase();

        return (
          invoiceNumber.includes(search) ||
          customerName.includes(search)
        );
      })
      .sort((a, b) => {
        const dateA = dayjs(
          getValue(
            a,
            "invoiceDate",
            "InvoiceDate"
          )
        );

        const dateB = dayjs(
          getValue(
            b,
            "invoiceDate",
            "InvoiceDate"
          )
        );

        return (
          dateB.valueOf() -
          dateA.valueOf()
        );
      });
  }, [
    invoices,
    searchText,
    customerId,
    warehouseId,
    statusId,
    dateRange,
    customerMap,
  ]);

  const postedInvoices = useMemo(() => {
    return filteredInvoices.filter(
      (invoice) => {
        const status =
          getInvoiceStatus(invoice);

        return (
          status.code ===
          STATUS_CODES.POSTED
        );
      }
    );
  }, [
    filteredInvoices,
    getInvoiceStatus,
  ]);

  const grandTotal = useMemo(() => {
    return postedInvoices.reduce(
      (sum, invoice) =>
        sum +
        safeNumber(
          getValue(
            invoice,
            "totalAmount",
            "TotalAmount"
          )
        ),
      0
    );
  }, [postedInvoices]);

  const dailySummary = useMemo(() => {
    const map = new Map();

    postedInvoices.forEach(
      (invoice) => {
        const date = dayjs(
          getValue(
            invoice,
            "invoiceDate",
            "InvoiceDate"
          )
        ).format("YYYY-MM-DD");

        const current =
          map.get(date) || {
            key: date,
            date,
            count: 0,
            total: 0,
          };

        current.count += 1;

        current.total += safeNumber(
          getValue(
            invoice,
            "totalAmount",
            "TotalAmount"
          )
        );

        map.set(date, current);
      }
    );

    return Array.from(
      map.values()
    ).sort(
      (a, b) =>
        dayjs(b.date).valueOf() -
        dayjs(a.date).valueOf()
    );
  }, [postedInvoices]);

  const handlePrintInvoice =
    useCallback(
      async (invoice) => {
        const invoiceId =
          getValue(
            invoice,
            "salesInvoiceId",
            "SalesInvoiceId"
          );

        if (!invoiceId) {
          messageApi.error(
            "Invoice ID not found."
          );
          return;
        }

        try {
          messageApi.loading({
            content:
              "Preparing invoice...",
            key: "print-invoice",
          });

          const [
            invoiceData,
            itemData,
            paymentData,
          ] = await Promise.all([
            getSalesInvoice(invoiceId),
            getSalesInvoiceItemsByInvoice(
              invoiceId
            ),
            getSalesPaymentsByInvoice(
              invoiceId
            ),
          ]);

          const items =
            Array.isArray(itemData)
              ? itemData
              : [];

          const payments =
            Array.isArray(paymentData)
              ? paymentData
              : [];

          const invoiceNumber =
            getValue(
              invoiceData,
              "invoiceNumber",
              "InvoiceNumber"
            ) || "-";

          const invoiceDate =
            getValue(
              invoiceData,
              "invoiceDate",
              "InvoiceDate"
            );

          const customerIdValue =
            safeNumber(
              getValue(
                invoiceData,
                "customerId",
                "CustomerId"
              )
            );

          const customerName =
            customerMap.get(
              customerIdValue
            ) ||
            "Walk-in Customer";

          const warehouseIdValue =
            safeNumber(
              getValue(
                invoiceData,
                "warehouseId",
                "WarehouseId"
              )
            );

          const warehouseName =
            getShortWarehouseName(
              warehouseMap.get(
                warehouseIdValue
              )
            );

          const currencyIdValue =
            safeNumber(
              getValue(
                invoiceData,
                "currencyId",
                "CurrencyId"
              )
            );

          const currencyName =
            currencyMap.get(
              currencyIdValue
            ) || "";

          const exchangeRate =
            safeNumber(
              getValue(
                invoiceData,
                "exchangeRate",
                "ExchangeRate"
              )
            ) || 1;

          const subTotal =
            safeNumber(
              getValue(
                invoiceData,
                "subTotal",
                "SubTotal"
              )
            );

          const discount =
            safeNumber(
              getValue(
                invoiceData,
                "discountAmount",
                "DiscountAmount"
              )
            );

          const tax =
            safeNumber(
              getValue(
                invoiceData,
                "taxAmount",
                "TaxAmount"
              )
            );

          const total =
            safeNumber(
              getValue(
                invoiceData,
                "totalAmount",
                "TotalAmount"
              )
            );

          const paid =
            payments.reduce(
              (sum, payment) =>
                sum +
                safeNumber(
                  getValue(
                    payment,
                    "amount",
                    "Amount"
                  )
                ),
              0
            );

          const balance =
            Math.max(
              total - paid,
              0
            );

          const change =
            Math.max(
              paid - total,
              0
            );

          const rowsHtml =
            items
              .map(
                (item, index) => {
                  const productId =
                    safeNumber(
                      getValue(
                        item,
                        "productId",
                        "ProductId"
                      )
                    );

                  const product =
                    productMap.get(
                      productId
                    );

                  const productName =
                    product?.name ||
                    `Product #${productId}`;

                  const sku =
                    product?.sku ||
                    "-";

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

                  const itemDiscount =
                    safeNumber(
                      getValue(
                        item,
                        "discountAmount",
                        "DiscountAmount"
                      )
                    );

                  const itemTax =
                    safeNumber(
                      getValue(
                        item,
                        "taxAmount",
                        "TaxAmount"
                      )
                    );

                  const itemTotal =
                    safeNumber(
                      getValue(
                        item,
                        "totalAmount",
                        "TotalAmount"
                      )
                    );

                  return `
                    <tr>
                      <td>${index + 1}</td>

                      <td>
                        ${escapeHtml(
                          productName
                        )}

                        <div class="sku">
                          SKU:
                          ${escapeHtml(
                            sku
                          )}
                        </div>
                      </td>

                      <td class="number">
                        ${quantity.toLocaleString(
                          "en-US"
                        )}
                      </td>

                      <td class="number">
                        ${formatMoney(
                          unitPrice
                        )}
                      </td>

                      <td class="number">
                        ${formatMoney(
                          itemDiscount
                        )}
                      </td>

                      <td class="number">
                        ${formatMoney(
                          itemTax
                        )}
                      </td>

                      <td class="number">
                        ${formatMoney(
                          itemTotal
                        )}
                      </td>
                    </tr>
                  `;
                }
              )
              .join("");

          const paymentRows =
            payments.length > 0
              ? payments
                  .map(
                    (payment) => {
                      const method =
                        getValue(
                          payment,
                          "paymentMethodName",
                          "PaymentMethodName",
                          "paymentMethodCode",
                          "PaymentMethodCode"
                        ) ||
                        `Payment Method #${safeNumber(
                          getValue(
                            payment,
                            "paymentMethodId",
                            "PaymentMethodId"
                          )
                        )}`;

                      const amount =
                        safeNumber(
                          getValue(
                            payment,
                            "amount",
                            "Amount"
                          )
                        );

                      const paymentDate =
                        getValue(
                          payment,
                          "paymentDate",
                          "PaymentDate"
                        );

                      return `
                        <tr>
                          <td>
                            ${escapeHtml(
                              method
                            )}
                          </td>

                          <td>
                            ${
                              paymentDate
                                ? dayjs(
                                    paymentDate
                                  ).format(
                                    "DD/MM/YYYY HH:mm"
                                  )
                                : "-"
                            }
                          </td>

                          <td class="number">
                            ${formatMoney(
                              amount
                            )}
                          </td>
                        </tr>
                      `;
                    }
                  )
                  .join("")
              : `
                  <tr>
                    <td colspan="3">
                      No payment record
                    </td>
                  </tr>
                `;

          const status =
            getInvoiceStatus(
              invoiceData
            );

          const printWindow =
            window.open(
              "",
              "_blank",
              "width=1000,height=800"
            );

          if (!printWindow) {
            messageApi.error({
              content:
                "Unable to open print window. Please allow pop-ups.",
              key: "print-invoice",
            });

            return;
          }

          printWindow.document.write(`
            <!DOCTYPE html>

            <html>
              <head>
                <meta charset="UTF-8" />

                <title>
                  Invoice ${escapeHtml(
                    invoiceNumber
                  )}
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

                    color: #222;

                    background: #fff;

                    font-size: 13px;
                  }

                  .invoice {
                    max-width: 1000px;

                    margin: 0 auto;
                  }

                  .header {
                    display: flex;

                    justify-content:
                      space-between;

                    align-items: flex-start;

                    margin-bottom: 25px;

                    padding-bottom: 15px;

                    border-bottom:
                      2px solid #222;
                  }

                  .title {
                    font-size: 28px;

                    font-weight: 700;

                    margin-bottom: 8px;
                  }

                  .invoice-number {
                    font-size: 15px;

                    font-weight: 600;
                  }

                  .header-right {
                    text-align: right;

                    line-height: 1.7;
                  }

                  .info {
                    display: grid;

                    grid-template-columns:
                      1fr 1fr;

                    gap: 20px;

                    margin-bottom: 20px;
                  }

                  .info-box {
                    border:
                      1px solid #ddd;

                    padding: 12px;

                    border-radius: 6px;
                  }

                  .info-title {
                    font-weight: 700;

                    margin-bottom: 8px;
                  }

                  .info-row {
                    display: flex;

                    margin-bottom: 5px;
                  }

                  .label {
                    width: 100px;

                    font-weight: 600;
                  }

                  .status {
                    display: inline-block;

                    margin-top: 5px;

                    padding:
                      4px 10px;

                    border-radius: 12px;

                    background: #f0f0f0;
                  }

                  table {
                    width: 100%;

                    border-collapse:
                      collapse;

                    margin-top: 15px;
                  }

                  th,
                  td {
                    border:
                      1px solid #ddd;

                    padding: 8px;
                  }

                  th {
                    background: #f5f5f5;

                    font-weight: 700;

                    text-align: left;
                  }

                  .number {
                    text-align: right;

                    white-space: nowrap;
                  }

                  .sku {
                    margin-top: 3px;

                    color: #777;

                    font-size: 11px;
                  }

                  .summary {
                    width: 350px;

                    margin-left: auto;

                    margin-top: 20px;
                  }

                  .summary-row {
                    display: flex;

                    justify-content:
                      space-between;

                    padding: 5px 0;
                  }

                  .summary-row.total {
                    border-top:
                      2px solid #222;

                    margin-top: 6px;

                    padding-top: 10px;

                    font-size: 18px;

                    font-weight: 700;
                  }

                  .summary-row.balance {
                    font-weight: 600;
                  }

                  .payment-title {
                    font-size: 16px;

                    font-weight: 700;

                    margin-top: 30px;
                  }

                  .footer {
                    text-align: center;

                    margin-top: 45px;

                    padding-top: 15px;

                    border-top:
                      1px solid #ddd;

                    color: #777;
                  }

                  .exchange {
                    color: #777;

                    font-size: 11px;

                    margin-top: 3px;
                  }

                  @media print {
                    body {
                      padding: 0;
                    }

                    .invoice {
                      max-width: none;
                    }

                    @page {
                      margin: 12mm;
                    }
                  }
                </style>
              </head>

              <body>
                <div class="invoice">

                  <div class="header">

                    <div>
                      <div class="title">
                        SALES INVOICE
                      </div>

                      <div class="invoice-number">
                        Invoice No:
                        ${escapeHtml(
                          invoiceNumber
                        )}
                      </div>

                      <div class="status">
                        ${escapeHtml(
                          status.name
                        )}
                      </div>
                    </div>

                    <div class="header-right">
                      <div>
                        <strong>Date:</strong>
                        ${
                          invoiceDate
                            ? dayjs(
                                invoiceDate
                              ).format(
                                "DD/MM/YYYY HH:mm"
                              )
                            : "-"
                        }
                      </div>

                      <div>
                        <strong>Currency:</strong>
                        ${escapeHtml(
                          currencyName
                        )}
                      </div>

                      <div class="exchange">
                        Exchange Rate:
                        ${formatMoney(
                          exchangeRate
                        )}
                      </div>
                    </div>

                  </div>

                  <div class="info">

                    <div class="info-box">
                      <div class="info-title">
                        Customer
                      </div>

                      <div class="info-row">
                        <div class="label">
                          Name
                        </div>

                        <div>
                          ${escapeHtml(
                            customerName
                          )}
                        </div>
                      </div>
                    </div>

                    <div class="info-box">
                      <div class="info-title">
                        Warehouse
                      </div>

                      <div class="info-row">
                        <div class="label">
                          Warehouse
                        </div>

                        <div>
                          ${escapeHtml(
                            warehouseName
                          )}
                        </div>
                      </div>
                    </div>

                  </div>

                  <table>
                    <thead>
                      <tr>
                        <th style="width: 45px;">
                          #
                        </th>

                        <th>
                          Product
                        </th>

                        <th
                          class="number"
                        >
                          Qty
                        </th>

                        <th
                          class="number"
                        >
                          Unit Price
                        </th>

                        <th
                          class="number"
                        >
                          Discount
                        </th>

                        <th
                          class="number"
                        >
                          Tax
                        </th>

                        <th
                          class="number"
                        >
                          Total
                        </th>
                      </tr>
                    </thead>

                    <tbody>
                      ${rowsHtml}
                    </tbody>
                  </table>

                  <div class="summary">

                    <div class="summary-row">
                      <span>
                        Subtotal
                      </span>

                      <span>
                        ${formatMoney(
                          subTotal
                        )}
                      </span>
                    </div>

                    <div class="summary-row">
                      <span>
                        Discount
                      </span>

                      <span>
                        ${formatMoney(
                          discount
                        )}
                      </span>
                    </div>

                    <div class="summary-row">
                      <span>
                        Tax
                      </span>

                      <span>
                        ${formatMoney(
                          tax
                        )}
                      </span>
                    </div>

                    <div
                      class="summary-row total"
                    >
                      <span>
                        Total
                      </span>

                      <span>
                        ${formatMoney(
                          total
                        )}
                      </span>
                    </div>

                    <div class="summary-row">
                      <span>
                        Paid
                      </span>

                      <span>
                        ${formatMoney(
                          paid
                        )}
                      </span>
                    </div>

                    <div
                      class="summary-row balance"
                    >
                      <span>
                        Balance
                      </span>

                      <span>
                        ${formatMoney(
                          balance
                        )}
                      </span>
                    </div>

                    ${
                      change > 0
                        ? `
                          <div
                            class="summary-row"
                          >
                            <span>
                              Change
                            </span>

                            <span>
                              ${formatMoney(
                                change
                              )}
                            </span>
                          </div>
                        `
                        : ""
                    }

                  </div>

                  <div class="payment-title">
                    Payments
                  </div>

                  <table>
                    <thead>
                      <tr>
                        <th>
                          Payment Method
                        </th>

                        <th>
                          Date
                        </th>

                        <th
                          class="number"
                        >
                          Amount
                        </th>
                      </tr>
                    </thead>

                    <tbody>
                      ${paymentRows}
                    </tbody>
                  </table>

                  <div class="footer">
                    Thank you for your business.
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

          messageApi.success({
            content:
              "Invoice ready to print.",
            key: "print-invoice",
          });
        } catch (error) {
          console.error(
            "FAILED TO PRINT SALES INVOICE:",
            error
          );

          console.error(
            "API RESPONSE:",
            error?.response?.data
          );

          messageApi.error({
            content:
              error?.response?.data
                ?.message ||
              error?.message ||
              "Failed to prepare invoice.",
            key: "print-invoice",
          });
        }
      },
      [
        customerMap,
        currencyMap,
        getInvoiceStatus,
        messageApi,
        productMap,
        warehouseMap,
      ]
    );

  const columns = [
    {
      title: "Invoice No.",
      key: "invoiceNumber",

      sorter: (a, b) =>
        String(
          getValue(
            a,
            "invoiceNumber",
            "InvoiceNumber"
          ) || ""
        ).localeCompare(
          String(
            getValue(
              b,
              "invoiceNumber",
              "InvoiceNumber"
            ) || ""
          )
        ),

      render: (_, record) =>
        getValue(
          record,
          "invoiceNumber",
          "InvoiceNumber"
        ) || "-",
    },

    {
      title: "Date",
      key: "invoiceDate",

      sorter: (a, b) =>
        dayjs(
          getValue(
            a,
            "invoiceDate",
            "InvoiceDate"
          )
        ).valueOf() -
        dayjs(
          getValue(
            b,
            "invoiceDate",
            "InvoiceDate"
          )
        ).valueOf(),

      render: (_, record) => {
        const date =
          getValue(
            record,
            "invoiceDate",
            "InvoiceDate"
          );

        return date
          ? dayjs(date).format(
              "DD/MM/YYYY HH:mm"
            )
          : "-";
      },
    },

    {
      title: "Customer",
      key: "customer",

      sorter: (a, b) => {
        const aId =
          safeNumber(
            getValue(
              a,
              "customerId",
              "CustomerId"
            )
          );

        const bId =
          safeNumber(
            getValue(
              b,
              "customerId",
              "CustomerId"
            )
          );

        return String(
          customerMap.get(aId) ||
            ""
        ).localeCompare(
          String(
            customerMap.get(bId) ||
              ""
          )
        );
      },

      render: (_, record) => {
        const id =
          safeNumber(
            getValue(
              record,
              "customerId",
              "CustomerId"
            )
          );

        return (
          customerMap.get(id) ||
          "Walk-in Customer"
        );
      },
    },

    {
      title: "Warehouse",
      key: "warehouse",

      render: (_, record) => {
        const id =
          safeNumber(
            getValue(
              record,
              "warehouseId",
              "WarehouseId"
            )
          );

        return getShortWarehouseName(
          warehouseMap.get(id)
        );
      },
    },

    {
      title: "Currency",
      key: "currency",

      render: (_, record) => {
        const id =
          safeNumber(
            getValue(
              record,
              "currencyId",
              "CurrencyId"
            )
          );

        return (
          currencyMap.get(id) ||
          "-"
        );
      },
    },

    {
      title: "Subtotal",
      key: "subTotal",
      align: "right",

      sorter: (a, b) =>
        safeNumber(
          getValue(
            a,
            "subTotal",
            "SubTotal"
          )
        ) -
        safeNumber(
          getValue(
            b,
            "subTotal",
            "SubTotal"
          )
        ),

      render: (_, record) =>
        formatMoney(
          getValue(
            record,
            "subTotal",
            "SubTotal"
          )
        ),
    },

    {
      title: "Discount",
      key: "discountAmount",
      align: "right",

      render: (_, record) =>
        formatMoney(
          getValue(
            record,
            "discountAmount",
            "DiscountAmount"
          )
        ),
    },

    {
      title: "Tax",
      key: "taxAmount",
      align: "right",

      render: (_, record) =>
        formatMoney(
          getValue(
            record,
            "taxAmount",
            "TaxAmount"
          )
        ),
    },

    {
      title: "Total",
      key: "totalAmount",
      align: "right",

      sorter: (a, b) =>
        safeNumber(
          getValue(
            a,
            "totalAmount",
            "TotalAmount"
          )
        ) -
        safeNumber(
          getValue(
            b,
            "totalAmount",
            "TotalAmount"
          )
        ),

      render: (_, record) =>
        formatMoney(
          getValue(
            record,
            "totalAmount",
            "TotalAmount"
          )
        ),
    },

    {
      title: "Status",
      key: "status",

      render: (_, record) => {
        const status =
          getInvoiceStatus(record);

        return (
          <Tag
            color={getStatusColor(
              status.code
            )}
          >
            {status.name}
          </Tag>
        );
      },
    },

    {
      title: "Print",
      key: "print",
      width: 75,
      align: "center",

      render: (_, record) => (
        <Button
          type="text"
          icon={
            <PrinterOutlined />
          }
          title="Print Invoice"
          onClick={(e) => {
            e.stopPropagation();

            void handlePrintInvoice(
              record
            );
          }}
        />
      ),
    },
  ];

  return (
    <>
      {contextHolder}

      <div
        className="sales-invoices-page"
        style={{
          padding: 24,
        }}
      >
        <Space
          orientation="horizontal"
          style={{
            width: "100%",
            justifyContent:
              "space-between",
            marginBottom: 16,
          }}
        >
          <div>
            <h2
              style={{
                margin: 0,
                fontSize: 24,
              }}
            >
              Sales Invoices
            </h2>

            <div
              style={{
                color: "#888",
                marginTop: 4,
              }}
            >
              Sales invoice management
            </div>
          </div>

          <Space orientation="horizontal">
            <Button
              icon={
                <ReloadOutlined />
              }
              onClick={loadData}
              loading={loading}
            >
              Refresh
            </Button>

            <Button
              type="primary"
              icon={
                <PlusOutlined />
              }
              onClick={onNew}
            >
              New Sale
            </Button>
          </Space>
        </Space>

        <Row
          gutter={[12, 12]}
          style={{
            marginBottom: 16,
          }}
        >
          <Col xs={24} md={8}>
            <Card size="small">
              <Statistic
                title="Sales Total"
                value={grandTotal}
                precision={2}
                styles={{
                  content: {
                    fontSize: 24,
                  },
                }}
              />
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card size="small">
              <Statistic
                title="Posted Invoices"
                value={
                  postedInvoices.length
                }
                styles={{
                  content: {
                    fontSize: 24,
                  },
                }}
              />
            </Card>
          </Col>

          <Col xs={24} md={8}>
            <Card size="small">
              <Statistic
                title="Invoices"
                value={
                  filteredInvoices.length
                }
                styles={{
                  content: {
                    fontSize: 24,
                  },
                }}
              />
            </Card>
          </Col>
        </Row>

        <Card
          size="small"
          style={{
            marginBottom: 16,
          }}
        >
          <Space
            orientation="horizontal"
            wrap
            style={{
              width: "100%",
            }}
          >
            <Input
              placeholder="Search invoice / customer"
              allowClear
              value={searchText}
              onChange={(e) =>
                setSearchText(
                  e.target.value
                )
              }
              style={{
                width: 240,
              }}
            />

            <Select
              value={dateFilter}
              onChange={
                handleDateFilterChange
              }
              style={{
                width: 150,
              }}
              options={[
                {
                  value: "all",
                  label: "All Dates",
                },
                {
                  value: "today",
                  label: "Today",
                },
                {
                  value: "yesterday",
                  label: "Yesterday",
                },
                {
                  value: "thisWeek",
                  label: "This Week",
                },
                {
                  value: "thisMonth",
                  label: "This Month",
                },
                {
                  value: "thisYear",
                  label: "This Year",
                },
                {
                  value: "custom",
                  label: "Custom",
                },
              ]}
            />

            <RangePicker
              value={dateRange}
              onChange={
                handleRangeChange
              }
              format="DD/MM/YYYY"
            />

            <Button
              icon={
                <LeftOutlined />
              }
              onClick={
                handlePreviousDay
              }
            />

            <Button
              icon={
                <RightOutlined />
              }
              onClick={
                handleNextDay
              }
            />

            <Select
              value={customerId}
              onChange={
                setCustomerId
              }
              style={{
                width: 190,
              }}
              showSearch
              optionFilterProp="label"
              options={[
                {
                  value: "all",
                  label: "All Customers",
                },

                ...customers.map(
                  (customer) => ({
                    value:
                      safeNumber(
                        getValue(
                          customer,
                          "customerId",
                          "CustomerId"
                        )
                      ),

                    label:
                      getValue(
                        customer,
                        "customerName",
                        "CustomerName",
                        "name",
                        "Name"
                      ) || "-",
                  })
                ),
              ]}
            />

            <Select
              value={warehouseId}
              onChange={
                setWarehouseId
              }
              style={{
                width: 160,
              }}
              options={[
                {
                  value: "all",
                  label:
                    "All Warehouses",
                },

                ...warehouses.map(
                  (warehouse) => ({
                    value:
                      safeNumber(
                        getValue(
                          warehouse,
                          "warehouseId",
                          "WarehouseId"
                        )
                      ),

                    label:
                      getShortWarehouseName(
                        getValue(
                          warehouse,
                          "warehouseName",
                          "WarehouseName",
                          "name",
                          "Name"
                        )
                      ),
                  })
                ),
              ]}
            />

            <Select
              value={statusId}
              onChange={setStatusId}
              style={{
                width: 150,
              }}
              options={[
                {
                  value: "all",
                  label: "All Status",
                },

                ...statuses
                  .filter(
                    (status) =>
                      String(
                        getValue(
                          status,
                          "documentType",
                          "DocumentType"
                        ) || ""
                      ).toUpperCase() ===
                      "SALES"
                  )
                  .map(
                    (status) => ({
                      value:
                        safeNumber(
                          getValue(
                            status,
                            "documentStatusId",
                            "DocumentStatusId"
                          )
                        ),

                      label:
                        getValue(
                          status,
                          "statusName",
                          "StatusName"
                        ) || "-",
                    })
                  ),
              ]}
            />
          </Space>
        </Card>

        <Card
          size="small"
          title="Sales Invoice List"
        >
          <Table
            rowKey={(record) =>
              getValue(
                record,
                "salesInvoiceId",
                "SalesInvoiceId"
              )
            }
            loading={loading}
            columns={columns}
            dataSource={
              filteredInvoices
            }
            pagination={{
              pageSize: 10,
              showSizeChanger: true,

              showTotal: (
                total,
                range
              ) =>
                `${range[0]}-${range[1]} of ${total}`,
            }}
            onRow={(record) => ({
              onDoubleClick: () => {
                const id =
                  getValue(
                    record,
                    "salesInvoiceId",
                    "SalesInvoiceId"
                  );

                if (id) {
                  onEdit(id);
                }
              },
            })}
            rowClassName={() =>
              "sales-invoice-row"
            }
            scroll={{
              x: 1350,
            }}
          />
        </Card>

        <Divider />

        <Card
          size="small"
          title="Daily Sales Total"
        >
          <Table
            size="small"
            rowKey="key"
            pagination={false}
            dataSource={
              dailySummary
            }
            columns={[
              {
                title: "Date",
                dataIndex: "date",

                render: (value) =>
                  dayjs(
                    value
                  ).format(
                    "DD/MM/YYYY"
                  ),
              },

              {
                title:
                  "Posted Invoices",
                dataIndex: "count",
                align: "right",
              },

              {
                title:
                  "Daily Sales Total",
                dataIndex: "total",
                align: "right",

                render: (value) =>
                  formatMoney(
                    value
                  ),
              },
            ]}
          />

          <div
            style={{
              textAlign: "right",
              marginTop: 12,
              fontSize: 16,
              fontWeight: 600,
            }}
          >
            Grand Total:{" "}
            {formatMoney(
              grandTotal
            )}
          </div>
        </Card>
      </div>

      <style>
        {`
          .sales-invoice-row {
            cursor: pointer;
            transition:
              background-color 0.15s ease,
              box-shadow 0.15s ease;
          }

          .sales-invoice-row:hover > td {
            background: #f0f5ff !important;
          }

          .sales-invoice-row:hover {
            box-shadow:
              inset 3px 0 0 #1677ff;
          }

          .sales-invoice-row .ant-btn {
            transition:
              transform 0.15s ease;
          }

          .sales-invoice-row:hover
            .ant-btn {
            transform: scale(1.08);
          }
        `}
      </style>
    </>
  );
}
