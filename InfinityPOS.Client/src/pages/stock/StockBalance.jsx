import { useCallback, useEffect, useMemo, useState } from "react";
import dayjs from "dayjs";
import {
  App,
  Button,
  Card,
  Col,
  DatePicker,
  Empty,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
} from "antd";
import {
  ArrowUpOutlined,
  DatabaseOutlined,
  ReloadOutlined,
  SearchOutlined,
} from "@ant-design/icons";

import { getStockBalances } from "../../api/stockBalancesApi";
import { getStockMovements } from "../../api/stockMovementsApi";
import { getProducts } from "../../api/productsApi";
import { getWarehouses } from "../../api/warehousesApi";

const { RangePicker } = DatePicker;
const { Text } = Typography;

/* =========================================================
   MOVEMENT TYPES
========================================================= */

const MOVEMENT_TYPES = {
  1: {
    code: "PURCHASE_IN",
    name: "Purchase In",
    group: "IN",
    direction: "IN",
  },
  2: {
    code: "SALE_OUT",
    name: "Sale Out",
    group: "OUT",
    direction: "OUT",
  },
  3: {
    code: "ADJUSTMENT_IN",
    name: "Adjustment In",
    group: "ADJUST",
    direction: "IN",
  },
  4: {
    code: "ADJUSTMENT_OUT",
    name: "Adjustment Out",
    group: "ADJUST",
    direction: "OUT",
  },
  5: {
    code: "TRANSFER_IN",
    name: "Transfer In",
    group: "IN",
    direction: "IN",
  },
  6: {
    code: "TRANSFER_OUT",
    name: "Transfer Out",
    group: "OUT",
    direction: "OUT",
  },
  7: {
    code: "BUNDLE_SPLIT",
    name: "Bundle Split",
    group: "OUT",
    direction: "OUT",
  },
  8: {
    code: "RETURN_IN",
    name: "Return In",
    group: "RETURN",
    direction: "IN",
  },
  9: {
    code: "RETURN_OUT",
    name: "Return Out",
    group: "RETURN",
    direction: "OUT",
  },
};

/* =========================================================
   HELPERS
========================================================= */

function getValue(obj, ...keys) {
  if (!obj) {
    return null;
  }

  for (const key of keys) {
    if (
      obj[key] !== undefined &&
      obj[key] !== null &&
      obj[key] !== ""
    ) {
      return obj[key];
    }
  }

  return null;
}

function safeNumber(value, fallback = 0) {
  if (
    value === null ||
    value === undefined ||
    value === ""
  ) {
    return fallback;
  }

  const numberValue = Number(value);

  return Number.isFinite(numberValue)
    ? numberValue
    : fallback;
}

function formatNumber(value) {
  return safeNumber(value).toLocaleString(
    undefined,
    {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }
  );
}

function formatDate(value) {
  if (!value) {
    return "-";
  }

  const date = dayjs(value);

  if (!date.isValid()) {
    return "-";
  }

  return date.format("DD/MM/YYYY HH:mm");
}

function getMovementType(movement) {
  const typeId = safeNumber(
    getValue(
      movement,
      "stockMovementTypeId",
      "StockMovementTypeId"
    )
  );

  return (
    MOVEMENT_TYPES[typeId] || {
      code: "UNKNOWN",
      name: "Unknown",
      group: "OUT",
      direction: "OUT",
    }
  );
}

function getWarehouseShortName(name) {
  if (!name) {
    return "-";
  }

  let value = String(name).trim();

  value = value
    .replace(/Main Warehouse/gi, "Main")
    .replace(/Warehouse/gi, "WH")
    .replace(/Branch/gi, "Br");

  if (value.length > 14) {
    value = `${value.substring(0, 12)}…`;
  }

  return value;
}

/* =========================================================
   COMPONENT
========================================================= */

export default function StockBalance() {
  const { message } = App.useApp();

  const [loading, setLoading] = useState(false);

  const [balances, setBalances] = useState([]);
  const [movements, setMovements] = useState([]);
  const [products, setProducts] = useState([]);
  const [warehouses, setWarehouses] = useState([]);

  const [searchText, setSearchText] = useState("");

  const [warehouseId, setWarehouseId] =
    useState(null);

  const [productId, setProductId] =
    useState(null);

  const [movementTypeId, setMovementTypeId] =
    useState("all");

  const [dateRange, setDateRange] =
    useState([
      dayjs().startOf("day"),
      dayjs().endOf("day"),
    ]);

  const [dateFilter, setDateFilter] =
    useState("today");

  /* =========================================================
     LOAD DATA
  ========================================================= */

  const loadData = useCallback(async () => {
    try {
      setLoading(true);

      const [
        balanceData,
        movementData,
        productData,
        warehouseData,
      ] = await Promise.all([
        getStockBalances(),
        getStockMovements(),
        getProducts(true),
        getWarehouses(true),
      ]);

      setBalances(
        Array.isArray(balanceData)
          ? balanceData
          : []
      );

      setMovements(
        Array.isArray(movementData)
          ? movementData
          : []
      );

      setProducts(
        Array.isArray(productData)
          ? productData
          : []
      );

      setWarehouses(
        Array.isArray(warehouseData)
          ? warehouseData
          : []
      );
    } catch (error) {
      console.error(
        "Stock Balance Load Error:",
        error
      );

      console.error(
        "API Response:",
        error?.response?.data
      );

      message.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to load stock data."
      );
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadData();
    }, 0);

    return () => clearTimeout(timer);
  }, [loadData]);

  /* =========================================================
     PRODUCT MAP
  ========================================================= */

  const productMap = useMemo(() => {
    const map = {};

    products.forEach((product) => {
      const id = safeNumber(
        getValue(
          product,
          "productId",
          "ProductId"
        )
      );

      if (id > 0) {
        map[id] = product;
      }
    });

    return map;
  }, [products]);

  /* =========================================================
     WAREHOUSE MAP
  ========================================================= */

  const warehouseMap = useMemo(() => {
    const map = {};

    warehouses.forEach((warehouse) => {
      const id = safeNumber(
        getValue(
          warehouse,
          "warehouseId",
          "WarehouseId"
        )
      );

      if (id > 0) {
        map[id] = warehouse;
      }
    });

    return map;
  }, [warehouses]);

  /* =========================================================
     PRODUCT HELPERS
  ========================================================= */

  const getProductName = useCallback(
    (id) => {
      const product =
        productMap[safeNumber(id)];

      return (
        getValue(
          product,
          "productName",
          "ProductName",
          "name",
          "Name"
        ) ||
        `Product #${id}`
      );
    },
    [productMap]
  );

  const getProductCode = useCallback(
    (id) => {
      const product =
        productMap[safeNumber(id)];

      return (
        getValue(
          product,
          "sku",
          "SKU",
          "productCode",
          "ProductCode",
          "code",
          "Code"
        ) ||
        `P-${id}`
      );
    },
    [productMap]
  );

  const getWarehouseName = useCallback(
    (id) => {
      const warehouse =
        warehouseMap[safeNumber(id)];

      return (
        getValue(
          warehouse,
          "warehouseName",
          "WarehouseName",
          "name",
          "Name"
        ) ||
        `WH #${id}`
      );
    },
    [warehouseMap]
  );

  /* =========================================================
     DATE FILTER
  ========================================================= */

  const handleDateFilterChange = (value) => {
    setDateFilter(value);

    if (value === "all") {
      setDateRange(null);
      return;
    }

    if (value === "today") {
      setDateRange([
        dayjs().startOf("day"),
        dayjs().endOf("day"),
      ]);
      return;
    }

    if (value === "yesterday") {
      const date =
        dayjs().subtract(1, "day");

      setDateRange([
        date.startOf("day"),
        date.endOf("day"),
      ]);
      return;
    }

    if (value === "thisWeek") {
      setDateRange([
        dayjs().startOf("week"),
        dayjs().endOf("week"),
      ]);
      return;
    }

    if (value === "thisMonth") {
      setDateRange([
        dayjs().startOf("month"),
        dayjs().endOf("month"),
      ]);
    }
  };

  const handleRangeChange = (values) => {
    if (!values || values.length !== 2) {
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

  /* =========================================================
     BATCH STOCK SUMMARY
  ========================================================= */

  const stockSummary = useMemo(() => {
    return balances
      .filter((balance) => {
        const remaining =
          safeNumber(
            getValue(
              balance,
              "remainingQuantity",
              "RemainingQuantity"
            )
          );

        return remaining > 0;
      })
      .map((balance) => ({
        productStockBatchId:
          safeNumber(
            getValue(
              balance,
              "productStockBatchId",
              "ProductStockBatchId"
            )
          ),

        productId:
          safeNumber(
            getValue(
              balance,
              "productId",
              "ProductId"
            )
          ),

        warehouseId:
          safeNumber(
            getValue(
              balance,
              "warehouseId",
              "WarehouseId"
            )
          ),

        purchaseInvoiceItemId:
          safeNumber(
            getValue(
              balance,
              "purchaseInvoiceItemId",
              "PurchaseInvoiceItemId"
            )
          ),

        costPrice:
          safeNumber(
            getValue(
              balance,
              "costPrice",
              "CostPrice"
            )
          ),

        salePrice:
          safeNumber(
            getValue(
              balance,
              "salePrice",
              "SalePrice"
            )
          ),

        originalQuantity:
          safeNumber(
            getValue(
              balance,
              "originalQuantity",
              "OriginalQuantity"
            )
          ),

        remainingQuantity:
          safeNumber(
            getValue(
              balance,
              "remainingQuantity",
              "RemainingQuantity"
            )
          ),

        currencyId:
          safeNumber(
            getValue(
              balance,
              "currencyId",
              "CurrencyId"
            )
          ),

        createdAt:
          getValue(
            balance,
            "createdAt",
            "CreatedAt"
          ),
      }));
  }, [balances]);

  /* =========================================================
     FILTERED STOCK
  ========================================================= */

  const filteredStockSummary = useMemo(() => {
    const search =
      searchText.trim().toLowerCase();

    return stockSummary.filter((row) => {
      if (
        warehouseId !== null &&
        row.warehouseId !==
          safeNumber(warehouseId)
      ) {
        return false;
      }

      if (
        productId !== null &&
        row.productId !==
          safeNumber(productId)
      ) {
        return false;
      }

      if (search) {
        const code =
          getProductCode(
            row.productId
          ).toLowerCase();

        const name =
          getProductName(
            row.productId
          ).toLowerCase();

        const warehouse =
          getWarehouseName(
            row.warehouseId
          ).toLowerCase();

        const cost =
          String(row.costPrice);

        const sale =
          String(row.salePrice);

        const matched =
          code.includes(search) ||
          name.includes(search) ||
          warehouse.includes(search) ||
          cost.includes(search) ||
          sale.includes(search);

        if (!matched) {
          return false;
        }
      }

      return true;
    });
  }, [
    stockSummary,
    warehouseId,
    productId,
    searchText,
    getProductCode,
    getProductName,
    getWarehouseName,
  ]);

  /* =========================================================
     FILTERED MOVEMENTS
  ========================================================= */

  const filteredMovements = useMemo(() => {
    const search =
      searchText.trim().toLowerCase();

    return movements.filter((movement) => {
      const movementProductId =
        safeNumber(
          getValue(
            movement,
            "productId",
            "ProductId"
          )
        );

      const movementWarehouseId =
        safeNumber(
          getValue(
            movement,
            "warehouseId",
            "WarehouseId"
          )
        );

      const typeId =
        safeNumber(
          getValue(
            movement,
            "stockMovementTypeId",
            "StockMovementTypeId"
          )
        );

      const movementDate =
        getValue(
          movement,
          "movementDate",
          "MovementDate",
          "createdAt",
          "CreatedAt",
          "date",
          "Date"
        );

      if (dateRange && movementDate) {
        const date = dayjs(
          movementDate
        );

        if (
          date.isBefore(
            dateRange[0]
          ) ||
          date.isAfter(
            dateRange[1]
          )
        ) {
          return false;
        }
      }

      if (
        warehouseId !== null &&
        movementWarehouseId !==
          safeNumber(warehouseId)
      ) {
        return false;
      }

      if (
        productId !== null &&
        movementProductId !==
          safeNumber(productId)
      ) {
        return false;
      }

      if (
        movementTypeId !== "all" &&
        typeId !==
          safeNumber(movementTypeId)
      ) {
        return false;
      }

      if (search) {
        const text = [
          getProductCode(
            movementProductId
          ),
          getProductName(
            movementProductId
          ),
          getWarehouseName(
            movementWarehouseId
          ),
          getMovementType(
            movement
          ).name,
          getValue(
            movement,
            "referenceNo",
            "ReferenceNo",
            "reference",
            "Reference"
          ),
        ]
          .filter(Boolean)
          .join(" ")
          .toLowerCase();

        if (!text.includes(search)) {
          return false;
        }
      }

      return true;
    });
  }, [
    movements,
    dateRange,
    warehouseId,
    productId,
    movementTypeId,
    searchText,
    getProductCode,
    getProductName,
    getWarehouseName,
  ]);

  /* =========================================================
     BATCH TABLE DATA
========================================================= */

  const stockTableData = useMemo(() => {
    return filteredStockSummary.map(
      (row, index) => {
        /*
          IMPORTANT

          This table is batch based.

          IN      = Original Quantity
          OUT     = Original - Remaining
          ADJUST  = 0
          RETURN  = 0
          BALANCE = Remaining Quantity

          Later Sales / Adjustment / Return
          posting can update these movement
          values from StockMovements.
        */

        const original =
          safeNumber(
            row.originalQuantity
          );

        const remaining =
          safeNumber(
            row.remainingQuantity
          );

        const sold =
          Math.max(
            0,
            original - remaining
          );

        return {
          key:
            row.productStockBatchId ||
            `${row.productId}-${row.warehouseId}-${index}`,

          index: index + 1,

          code: getProductCode(
            row.productId
          ),

          name: getProductName(
            row.productId
          ),

          warehouse:
            getWarehouseShortName(
              getWarehouseName(
                row.warehouseId
              )
            ),

          productId:
            row.productId,

          warehouseId:
            row.warehouseId,

          productStockBatchId:
            row.productStockBatchId,

          inQty: original,

          outQty: sold,

          adjustQty: 0,

          returnQty: 0,

          balance: remaining,

          costPrice:
            row.costPrice,

          salePrice:
            row.salePrice,

          createdAt:
            row.createdAt,
        };
      }
    );
  }, [
    filteredStockSummary,
    getProductCode,
    getProductName,
    getWarehouseName,
  ]);

  /* =========================================================
     TOTALS
  ========================================================= */

  const currentStock =
    filteredStockSummary.reduce(
      (total, row) =>
        total +
        safeNumber(
          row.remainingQuantity
        ),
      0
    );

  const totalIn =
    filteredMovements.reduce(
      (total, movement) => {
        const type =
          getMovementType(
            movement
          );

        if (
          type.direction !== "IN"
        ) {
          return total;
        }

        return (
          total +
          Math.abs(
            safeNumber(
              getValue(
                movement,
                "quantity",
                "Quantity"
              )
            )
          )
        );
      },
      0
    );

  const totalOut =
    filteredMovements.reduce(
      (total, movement) => {
        const type =
          getMovementType(
            movement
          );

        if (
          type.direction !== "OUT"
        ) {
          return total;
        }

        return (
          total +
          Math.abs(
            safeNumber(
              getValue(
                movement,
                "quantity",
                "Quantity"
              )
            )
          )
        );
      },
      0
    );

  /* =========================================================
     MOVEMENT TABLE DATA
  ========================================================= */

  const movementTableData =
    useMemo(() => {
      return filteredMovements.map(
        (movement, index) => {
          const productId =
            safeNumber(
              getValue(
                movement,
                "productId",
                "ProductId"
              )
            );

          const warehouseId =
            safeNumber(
              getValue(
                movement,
                "warehouseId",
                "WarehouseId"
              )
            );

          const type =
            getMovementType(
              movement
            );

          return {
            key:
              getValue(
                movement,
                "stockMovementId",
                "StockMovementId"
              ) ||
              `${productId}-${warehouseId}-${index}`,

            date: formatDate(
              getValue(
                movement,
                "movementDate",
                "MovementDate",
                "createdAt",
                "CreatedAt"
              )
            ),

            code:
              getProductCode(
                productId
              ),

            name:
              getProductName(
                productId
              ),

            warehouse:
              getWarehouseShortName(
                getWarehouseName(
                  warehouseId
                )
              ),

            movement:
              type.name,

            direction:
              type.direction,

            quantity:
              Math.abs(
                safeNumber(
                  getValue(
                    movement,
                    "quantity",
                    "Quantity"
                  )
                )
              ),

            reference:
              getValue(
                movement,
                "referenceNo",
                "ReferenceNo",
                "reference",
                "Reference"
              ) || "-",
          };
        }
      );
    }, [
      filteredMovements,
      getProductCode,
      getProductName,
      getWarehouseName,
    ]);

  /* =========================================================
     OPTIONS
  ========================================================= */

  const warehouseOptions =
    useMemo(() => {
      return warehouses.map(
        (warehouse) => {
          const id =
            safeNumber(
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
            ) ||
            `WH #${id}`;

          return {
            value: id,
            label: name,
          };
        }
      );
    }, [warehouses]);

  const productOptions =
    useMemo(() => {
      return products.map(
        (product) => {
          const id =
            safeNumber(
              getValue(
                product,
                "productId",
                "ProductId"
              )
            );

          const code =
            getValue(
              product,
              "sku",
              "SKU",
              "productCode",
              "ProductCode",
              "code",
              "Code"
            ) || "";

          const name =
            getValue(
              product,
              "productName",
              "ProductName",
              "name",
              "Name"
            ) ||
            `Product #${id}`;

          return {
            value: id,
            label: code
              ? `${code} - ${name}`
              : name,
          };
        }
      );
    }, [products]);

  const movementTypeOptions =
    useMemo(() => {
      return [
        {
          value: "all",
          label: "All Movement Types",
        },

        ...Object.entries(
          MOVEMENT_TYPES
        ).map(
          ([id, type]) => ({
            value: safeNumber(id),
            label: type.name,
          })
        ),
      ];
    }, []);

  /* =========================================================
     STOCK COLUMNS
  ========================================================= */

  const stockColumns = [
    {
      title: "#",
      dataIndex: "index",
      key: "index",
      width: 45,
      align: "center",
    },

    {
      title: "CODE",
      dataIndex: "code",
      key: "code",
      width: 110,
      ellipsis: true,
    },

    {
      title: "NAME",
      dataIndex: "name",
      key: "name",
      width: 220,
      ellipsis: true,
    },

    {
      title: "WH",
      dataIndex: "warehouse",
      key: "warehouse",
      width: 75,
      align: "center",
    },

    {
      title: "IN",
      dataIndex: "inQty",
      key: "inQty",
      width: 80,
      align: "right",

      render: (value) =>
        formatNumber(value),
    },

    {
      title: "OUT",
      dataIndex: "outQty",
      key: "outQty",
      width: 80,
      align: "right",

      render: (value) =>
        formatNumber(value),
    },

    {
      title: "ADJUST",
      dataIndex: "adjustQty",
      key: "adjustQty",
      width: 90,
      align: "right",

      render: (value) =>
        formatNumber(value),
    },

    {
      title: "RETURN",
      dataIndex: "returnQty",
      key: "returnQty",
      width: 90,
      align: "right",

      render: (value) =>
        formatNumber(value),
    },

    {
      title: "BALANCE",
      dataIndex: "balance",
      key: "balance",
      width: 100,
      align: "right",

      render: (value) => (
        <Text strong>
          {formatNumber(value)}
        </Text>
      ),
    },

    {
      title: "COST",
      dataIndex: "costPrice",
      key: "costPrice",
      width: 100,
      align: "right",

      render: (value) =>
        formatNumber(value),
    },

    {
      title: "SALE",
      dataIndex: "salePrice",
      key: "salePrice",
      width: 100,
      align: "right",

      render: (value) => (
        <Text strong>
          {formatNumber(value)}
        </Text>
      ),
    },
  ];

  /* =========================================================
     MOVEMENT COLUMNS
  ========================================================= */

  const movementColumns = [
    {
      title: "DATE",
      dataIndex: "date",
      key: "date",
      width: 140,
    },

    {
      title: "CODE",
      dataIndex: "code",
      key: "code",
      width: 100,
    },

    {
      title: "NAME",
      dataIndex: "name",
      key: "name",
      width: 200,
      ellipsis: true,
    },

    {
      title: "WH",
      dataIndex: "warehouse",
      key: "warehouse",
      width: 70,
      align: "center",
    },

    {
      title: "MOVEMENT",
      dataIndex: "movement",
      key: "movement",
      width: 130,

      render: (
        value,
        record
      ) => {
        let color = "default";

        if (
          record.direction ===
          "IN"
        ) {
          color = "success";
        }

        if (
          record.direction ===
          "OUT"
        ) {
          color = "error";
        }

        return (
          <Tag color={color}>
            {value}
          </Tag>
        );
      },
    },

    {
      title: "QTY",
      dataIndex: "quantity",
      key: "quantity",
      width: 85,
      align: "right",

      render: (
        value,
        record
      ) => {
        if (
          record.direction ===
          "IN"
        ) {
          return (
            <Text
              style={{
                color: "#3f8600",
                fontWeight: 600,
              }}
            >
              +{formatNumber(
                value
              )}
            </Text>
          );
        }

        return (
          <Text
            style={{
              color: "#cf1322",
              fontWeight: 600,
            }}
          >
            −{formatNumber(
              value
            )}
          </Text>
        );
      },
    },

    {
      title: "REFERENCE",
      dataIndex: "reference",
      key: "reference",
      width: 140,
      ellipsis: true,
    },
  ];

  /* =========================================================
     CLEAR FILTERS
  ========================================================= */

  const handleClearFilters = () => {
    setSearchText("");
    setWarehouseId(null);
    setProductId(null);
    setMovementTypeId("all");

    setDateFilter("today");

    setDateRange([
      dayjs().startOf("day"),
      dayjs().endOf("day"),
    ]);
  };

  /* =========================================================
     RENDER
  ========================================================= */

  return (
    <div>
      {/* ===================================================
          HEADER
      =================================================== */}

      <div
        style={{
          display: "flex",
          justifyContent:
            "space-between",
          alignItems: "center",
          marginBottom: 16,
        }}
      >
        <div>
          <Typography.Title
            level={3}
            style={{
              margin: 0,
            }}
          >
            <DatabaseOutlined /> Stock
            Balance
          </Typography.Title>

          <Text type="secondary">
            Stock balance by purchase batch
          </Text>
        </div>

        <Button
          icon={
            <ReloadOutlined />
          }
          onClick={loadData}
          loading={loading}
        >
          Refresh
        </Button>
      </div>

      {/* ===================================================
          FILTERS
      =================================================== */}

      <Card
        size="small"
        style={{
          marginBottom: 16,
        }}
      >
        <Row gutter={[10, 10]}>
          <Col
            xs={24}
            sm={12}
            md={5}
            lg={4}
          >
            <Select
              style={{
                width: "100%",
              }}
              value={dateFilter}
              onChange={
                handleDateFilterChange
              }
              options={[
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
                  value: "all",
                  label: "All",
                },
              ]}
            />
          </Col>

          <Col
            xs={24}
            sm={12}
            md={7}
            lg={7}
          >
            <RangePicker
              style={{
                width: "100%",
              }}
              value={
                dateRange
                  ? [
                      dateRange[0],
                      dateRange[1],
                    ]
                  : null
              }
              onChange={
                handleRangeChange
              }
            />
          </Col>

          <Col
            xs={24}
            sm={12}
            md={5}
            lg={4}
          >
            <Select
              allowClear
              placeholder="Warehouse"
              style={{
                width: "100%",
              }}
              value={warehouseId}
              onChange={
                setWarehouseId
              }
              options={
                warehouseOptions
              }
            />
          </Col>

          <Col
            xs={24}
            sm={12}
            md={6}
            lg={5}
          >
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Product"
              style={{
                width: "100%",
              }}
              value={productId}
              onChange={
                setProductId
              }
              options={
                productOptions
              }
            />
          </Col>

          <Col
            xs={24}
            sm={12}
            md={6}
            lg={5}
          >
            <Select
              style={{
                width: "100%",
              }}
              value={
                movementTypeId
              }
              onChange={
                setMovementTypeId
              }
              options={
                movementTypeOptions
              }
            />
          </Col>

          <Col
            xs={24}
            sm={12}
            md={8}
            lg={7}
          >
            <Input
              allowClear
              prefix={
                <SearchOutlined />
              }
              placeholder="Search code / name / price"
              value={searchText}
              onChange={(e) =>
                setSearchText(
                  e.target.value
                )
              }
            />
          </Col>

          <Col>
            <Button
              onClick={
                handleClearFilters
              }
            >
              Clear
            </Button>
          </Col>
        </Row>
      </Card>

      {/* ===================================================
          STATISTICS
      =================================================== */}

      <Row
        gutter={[12, 12]}
        style={{
          marginBottom: 16,
        }}
      >
        <Col
          xs={12}
          sm={6}
        >
          <Card size="small">
            <Statistic
              title="IN"
              value={totalIn}
              precision={2}
              prefix="+"
              styles={{
                content: {
                  color:
                    "#3f8600",
                  fontSize: 20,
                },
              }}
            />
          </Card>
        </Col>

        <Col
          xs={12}
          sm={6}
        >
          <Card size="small">
            <Statistic
              title="OUT"
              value={totalOut}
              precision={2}
              prefix="−"
              styles={{
                content: {
                  color:
                    "#cf1322",
                  fontSize: 20,
                },
              }}
            />
          </Card>
        </Col>

        <Col
          xs={12}
          sm={6}
        >
          <Card size="small">
            <Statistic
              title="CURRENT STOCK"
              value={currentStock}
              precision={2}
              styles={{
                content: {
                  fontSize: 20,
                },
              }}
            />
          </Card>
        </Col>

        <Col
          xs={12}
          sm={6}
        >
          <Card size="small">
            <Statistic
              title="BATCHES"
              value={
                filteredStockSummary.length
              }
              styles={{
                content: {
                  fontSize: 20,
                },
              }}
            />
          </Card>
        </Col>
      </Row>

      {/* ===================================================
          CURRENT STOCK BALANCE
      =================================================== */}

      <Card
        size="small"
        title={
          <Space>
            <DatabaseOutlined />
            <span>
              Current Stock Balance
            </span>
          </Space>
        }
        style={{
          marginBottom: 16,
        }}
      >
        {stockTableData.length ===
        0 ? (
          <Empty
            description="No stock balance found"
          />
        ) : (
          <Table
            size="small"
            bordered
            loading={loading}
            columns={stockColumns}
            dataSource={
              stockTableData
            }
            scroll={{
              x: 1100,
            }}
            pagination={{
              defaultPageSize: 100,
              pageSizeOptions: [
                100,
                200,
                500,
              ],
              showSizeChanger: true,
              showTotal:
                (total) =>
                  `Total ${total} batches`,
            }}
          />
        )}
      </Card>

      {/* ===================================================
          STOCK MOVEMENTS
      =================================================== */}

      <Card
        size="small"
        title={
          <Space>
            <ArrowUpOutlined />
            <span>
              Stock Movements
            </span>
          </Space>
        }
      >
        {movementTableData.length ===
        0 ? (
          <Empty
            description="No stock movement found"
          />
        ) : (
          <Table
            size="small"
            bordered
            loading={loading}
            columns={
              movementColumns
            }
            dataSource={
              movementTableData
            }
            scroll={{
              x: 900,
            }}
            pagination={{
              defaultPageSize: 100,
              pageSizeOptions: [
                100,
                200,
                500,
              ],
              showSizeChanger: true,
              showTotal:
                (total) =>
                  `Total ${total} movements`,
            }}
          />
        )}
      </Card>

      {/* ===================================================
          CSS
      =================================================== */}

      <style>
        {`
          .ant-table-small
            .ant-table-thead > tr > th {
            padding: 7px 8px !important;
            font-size: 12px;
            white-space: nowrap;
          }

          .ant-table-small
            .ant-table-tbody > tr > td {
            padding: 6px 8px !important;
            font-size: 12px;
          }

          .ant-table-tbody > tr:hover > td {
            background: #fafafa !important;
          }
        `}
      </style>
    </div>
  );
}