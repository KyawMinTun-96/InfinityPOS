import {
  Button,
  Card,
  Col,
  DatePicker,
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
  SaveOutlined,
  CheckOutlined,
  CloseOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import {
  useCallback,
  useEffect,
  useState,
} from "react";

import apiClient from "../../api/apiClient";
import {
  createExpense,
  getExpenseDetails,
  postExpense,
  updateExpense,
  cancelExpense,
} from "../../api/expensesApi";

const { Title, Text } = Typography;
const { TextArea } = Input;

export default function ExpenseForm({
  expenseId = null,
  onCancel,
  onSaved,
}) {
  const [form] = Form.useForm();
  const [messageApi, contextHolder] =
    message.useMessage();

  const [accounts, setAccounts] = useState([]);
  const [currencies, setCurrencies] = useState([]);

  const [loading, setLoading] = useState(false);
  const [lookupLoading, setLookupLoading] =
    useState(false);

  const [voucherNo, setVoucherNo] = useState("");
  const [status, setStatus] =
    useState("DRAFT");

  const [items, setItems] = useState([]);

  const isEdit = Boolean(expenseId);

  /*
   * ============================================================
   * LOAD LOOKUPS
   * ============================================================
   */

  const loadLookups = useCallback(async () => {
    try {
      setLookupLoading(true);

      const [
        accountsResponse,
        currenciesResponse,
      ] = await Promise.all([
        apiClient.get("/accounts", {
          params: {
            isActive: true,
          },
        }),

        apiClient.get("/currencies", {
          params: {
            isActive: true,
          },
        }),
      ]);

      setAccounts(
        accountsResponse.data || []
      );

      setCurrencies(
        currenciesResponse.data || []
      );
    } catch (error) {
      console.error(error);

      messageApi.error(
        error?.response?.data?.message ||
          "Failed to load accounts and currencies."
      );
    } finally {
      setLookupLoading(false);
    }
  }, [messageApi]);

  /*
   * ============================================================
   * RESET NEW FORM
   * ============================================================
   */

  const resetNewForm = useCallback(() => {
    form.resetFields();

    form.setFieldsValue({
      expenseDate: dayjs(),
      payeeName: "",
      currencyId: undefined,
      exchangeRate: 1,
      paymentAccountId: undefined,
      notes: "",
    });

    setVoucherNo("");
    setStatus("DRAFT");
    setItems([]);
  }, [form]);

  /*
   * ============================================================
   * LOAD EXISTING EXPENSE
   * ============================================================
   */

  const loadExpense = useCallback(
    async (id) => {
      try {
        setLoading(true);

        const response =
          await getExpenseDetails(id);

        const expense =
          response?.expense;

        const expenseItems =
          response?.items || [];

        if (!expense) {
          throw new Error(
            "Expense data not found."
          );
        }

        setVoucherNo(
          expense.expenseNumber || ""
        );

        setStatus(
          expense.statusCode ||
            "DRAFT"
        );

        form.setFieldsValue({
          expenseDate:
            expense.expenseDate
              ? dayjs(
                  expense.expenseDate
                )
              : dayjs(),

          payeeName:
            expense.payeeName || "",

          currencyId:
            expense.currencyId > 0
              ? expense.currencyId
              : undefined,

          exchangeRate:
            expense.exchangeRate > 0
              ? expense.exchangeRate
              : 1,

          paymentAccountId:
            expense.paymentAccountId ||
            undefined,

          notes:
            expense.notes || "",
        });

        setItems(
          expenseItems.map(
            (item, index) => ({
              key:
                item.expenseItemId ||
                `existing-${index}-${Date.now()}`,

              expenseItemId:
                item.expenseItemId,

              accountId:
                item.accountId,

              description:
                item.description || "",

              amount:
                Number(
                  item.amount || 0
                ),
            })
          )
        );
      } catch (error) {
        console.error(error);

        messageApi.error(
          error?.response?.data?.message ||
            error?.message ||
            "Failed to load expense."
        );
      } finally {
        setLoading(false);
      }
    },
    [form, messageApi]
  );

  /*
   * ============================================================
   * ADD ITEM
   * ============================================================
   */

  function addItem() {
    setItems((currentItems) => [
      ...currentItems,
      {
        key: `new-${Date.now()}-${Math.random()}`,
        expenseItemId: null,
        accountId: undefined,
        description: "",
        amount: 0,
      },
    ]);
  }

  /*
   * ============================================================
   * REMOVE ITEM
   * ============================================================
   */

  function removeItem(key) {
    setItems((currentItems) =>
      currentItems.filter(
        (item) => item.key !== key
      )
    );
  }

  /*
   * ============================================================
   * UPDATE ITEM
   * ============================================================
   */

  function updateItem(
    key,
    field,
    value
  ) {
    setItems((currentItems) =>
      currentItems.map((item) =>
        item.key === key
          ? {
              ...item,
              [field]: value,
            }
          : item
      )
    );
  }

  /*
   * ============================================================
   * TOTAL
   * ============================================================
   */

  const totalAmount = items.reduce(
    (sum, item) =>
      sum +
      Number(item.amount || 0),
    0
  );

  /*
   * ============================================================
   * SAVE
   * ============================================================
   */

  async function handleSave() {
    try {
      const values =
        await form.validateFields();

      if (items.length === 0) {
        messageApi.warning(
          "Please add at least one expense item."
        );
        return;
      }

      const invalidAccount =
        items.some(
          (item) => !item.accountId
        );

      if (invalidAccount) {
        messageApi.warning(
          "Please select an account for every expense item."
        );
        return;
      }

      const invalidAmount =
        items.some(
          (item) =>
            Number(
              item.amount || 0
            ) <= 0
        );

      if (invalidAmount) {
        messageApi.warning(
          "Every expense item amount must be greater than zero."
        );
        return;
      }

      const payload = {
        expenseDate:
          values.expenseDate
            ? values.expenseDate.toISOString()
            : new Date().toISOString(),

        payeeName:
          values.payeeName || null,

        currencyId:
          values.currencyId,

        exchangeRate:
          Number(
            values.exchangeRate || 1
          ),

        paymentAccountId:
          values.paymentAccountId,

        totalAmount,

        notes:
          values.notes || null,

        items: items.map(
          (item) => ({
            accountId:
              item.accountId,

            description:
              item.description ||
              null,

            amount:
              Number(
                item.amount || 0
              ),
          })
        ),
      };

      setLoading(true);

      let savedExpense;

      if (isEdit) {
        savedExpense =
          await updateExpense(
            expenseId,
            payload
          );
      } else {
        savedExpense =
          await createExpense(
            payload
          );
      }

      const savedVoucher =
        savedExpense?.expenseNumber ||
        savedExpense?.ExpenseNumber ||
        "";

      if (savedVoucher) {
        setVoucherNo(
          savedVoucher
        );
      }

      messageApi.success(
        isEdit
          ? "Expense updated successfully."
          : "Expense saved successfully."
      );

      if (onSaved) {
        onSaved(savedExpense);
      }
    } catch (error) {
      console.error(error);

      if (error?.errorFields) {
        return;
      }

      messageApi.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to save expense."
      );
    } finally {
      setLoading(false);
    }
  }

  /*
   * ============================================================
   * POST
   * ============================================================
   */

  async function handlePost() {
    if (!expenseId) {
      messageApi.warning(
        "Please save the expense before posting."
      );
      return;
    }

    try {
      const values =
        await form.validateFields();

      if (items.length === 0) {
        messageApi.warning(
          "Please add at least one expense item."
        );
        return;
      }

      const invalidAccount =
        items.some(
          (item) => !item.accountId
        );

      if (invalidAccount) {
        messageApi.warning(
          "Please select an account for every expense item."
        );
        return;
      }

      const invalidAmount =
        items.some(
          (item) =>
            Number(
              item.amount || 0
            ) <= 0
        );

      if (invalidAmount) {
        messageApi.warning(
          "Every expense item amount must be greater than zero."
        );
        return;
      }

      const payload = {
        expenseDate:
          values.expenseDate
            ? values.expenseDate.toISOString()
            : new Date().toISOString(),

        payeeName:
          values.payeeName || null,

        currencyId:
          values.currencyId,

        exchangeRate:
          Number(
            values.exchangeRate || 1
          ),

        paymentAccountId:
          values.paymentAccountId,

        totalAmount,

        notes:
          values.notes || null,

        items: items.map(
          (item) => ({
            accountId:
              item.accountId,

            description:
              item.description ||
              null,

            amount:
              Number(
                item.amount || 0
              ),
          })
        ),
      };

      setLoading(true);

      await updateExpense(
        expenseId,
        payload
      );

      const postedExpense =
        await postExpense(
          expenseId
        );

      setStatus("POSTED");

      messageApi.success(
        "Expense posted successfully."
      );

      if (onSaved) {
        onSaved(postedExpense);
      }
    } catch (error) {
      console.error(error);

      if (error?.errorFields) {
        return;
      }

      messageApi.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to post expense."
      );
    } finally {
      setLoading(false);
    }
  }

  /*
   * ============================================================
   * CANCEL EXPENSE
   * ============================================================
   */

  async function handleCancelExpense() {
    if (!expenseId) {
      onCancel?.();
      return;
    }

    try {
      setLoading(true);

      await cancelExpense(
        expenseId
      );

      messageApi.success(
        "Expense cancelled successfully."
      );

      if (onSaved) {
        onSaved();
      }
    } catch (error) {
      console.error(error);

      messageApi.error(
        error?.response?.data?.message ||
          error?.message ||
          "Failed to cancel expense."
      );
    } finally {
      setLoading(false);
    }
  }

  /*
   * ============================================================
   * EFFECT - LOOKUPS
   * ============================================================
   */

  useEffect(() => {
    const timer =
      setTimeout(() => {
        loadLookups();
      }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [loadLookups]);

  /*
   * ============================================================
   * EFFECT - NEW / EDIT
   * ============================================================
   */

  useEffect(() => {
    const timer =
      setTimeout(() => {
        if (expenseId) {
          loadExpense(
            expenseId
          );
        } else {
          resetNewForm();
        }
      }, 0);

    return () => {
      clearTimeout(timer);
    };
  }, [
    expenseId,
    loadExpense,
    resetNewForm,
  ]);

  /*
   * ============================================================
   * TABLE COLUMNS
   * ============================================================
   */

  const itemColumns = [
    {
      title: "#",
      width: 60,
      align: "center",

      render: (
        _,
        __,
        index
      ) => index + 1,
    },

    {
      title: "Expense Account",
      dataIndex: "accountId",
      width: 300,

      render: (
        value,
        record
      ) => (
        <Select
          showSearch
          allowClear
          placeholder="Select account"
          value={value}
          loading={
            lookupLoading
          }
          disabled={
            status !== "DRAFT"
          }
          style={{
            width: "100%",
          }}
          optionFilterProp="label"
          options={accounts.map(
            (account) => ({
              value:
                account.accountId,

              label:
                `${account.accountCode} - ${account.accountName}`,
            })
          )}
          onChange={(
            newValue
          ) =>
            updateItem(
              record.key,
              "accountId",
              newValue
            )
          }
        />
      ),
    },

    {
      title: "Description",
      dataIndex: "description",
      width: 380,

      render: (
        value,
        record
      ) => (
        <Input
          value={value}
          disabled={
            status !== "DRAFT"
          }
          placeholder="Description"
          onChange={(
            event
          ) =>
            updateItem(
              record.key,
              "description",
              event.target.value
            )
          }
        />
      ),
    },

    {
      title: "Amount",
      dataIndex: "amount",
      width: 180,
      align: "right",

      render: (
        value,
        record
      ) => (
        <InputNumber
          value={value}
          min={0}
          precision={2}
          disabled={
            status !== "DRAFT"
          }
          style={{
            width: "100%",
          }}
          onChange={(
            newValue
          ) =>
            updateItem(
              record.key,
              "amount",
              Number(
                newValue || 0
              )
            )
          }
        />
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
          disabled={
            status !== "DRAFT"
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

  const isDraft =
    status === "DRAFT";

  const isPosted =
    status === "POSTED";

  const isCancelled =
    status === "CANCELLED";

  /*
   * ============================================================
   * RENDER
   * ============================================================
   */

  return (
    <>
      {contextHolder}

      <div
        style={{
          padding: 24,
          background: "#f5f5f5",
          minHeight: "100%",
        }}
      >
        <div
          style={{
            marginBottom: 20,
            display: "flex",
            justifyContent:
              "space-between",
            alignItems:
              "flex-start",
            gap: 16,
          }}
        >
          <div>
            <Title
              level={2}
              style={{
                margin: 0,
              }}
            >
              {isEdit
                ? "Edit Expense"
                : "New Expense"}
            </Title>

            <Text type="secondary">
              Expense transaction
              management
            </Text>
          </div>

          <Space>
            <Button
              icon={
                <CloseOutlined />
              }
              onClick={
                onCancel
              }
            >
              Back
            </Button>

            {isDraft && (
              <>
                <Button
                  type="primary"
                  icon={
                    <SaveOutlined />
                  }
                  loading={
                    loading
                  }
                  onClick={
                    handleSave
                  }
                >
                  Save Draft
                </Button>

                {isEdit && (
                  <Button
                    type="primary"
                    icon={
                      <CheckOutlined />
                    }
                    loading={
                      loading
                    }
                    onClick={
                      handlePost
                    }
                  >
                    Post
                  </Button>
                )}
              </>
            )}

            {isDraft &&
              isEdit && (
                <Button
                  danger
                  icon={
                    <CloseOutlined />
                  }
                  loading={
                    loading
                  }
                  onClick={
                    handleCancelExpense
                  }
                >
                  Cancel Expense
                </Button>
              )}
          </Space>
        </div>

        <Card
          title="Expense Information"
          style={{
            marginBottom: 20,
          }}
        >
          <Form
            form={form}
            layout="vertical"
            requiredMark="optional"
          >
            <Row gutter={16}>
              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  label="Voucher No."
                >
                  <Input
                    value={
                      voucherNo
                    }
                    placeholder={
                      isEdit
                        ? "Loading..."
                        : "Auto generated after save"
                    }
                    disabled
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  name="expenseDate"
                  label="Expense Date"
                  rules={[
                    {
                      required: true,
                      message:
                        "Please select expense date.",
                    },
                  ]}
                >
                  <DatePicker
                    style={{
                      width: "100%",
                    }}
                    disabled={
                      !isDraft
                    }
                    format="DD/MM/YYYY"
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  name="payeeName"
                  label="Payee Name"
                >
                  <Input
                    disabled={
                      !isDraft
                    }
                    placeholder="Enter payee name"
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  name="currencyId"
                  label="Currency"
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
                    allowClear
                    loading={
                      lookupLoading
                    }
                    disabled={
                      !isDraft
                    }
                    placeholder="Select currency"
                    optionFilterProp="label"
                    options={currencies.map(
                      (
                        currency
                      ) => ({
                        value:
                          currency.currencyId,

                        label:
                          `${currency.currencyCode} - ${currency.currencyName}`,
                      })
                    )}
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col
                xs={24}
                md={6}
              >
                <Form.Item
                  name="exchangeRate"
                  label="Exchange Rate"
                  rules={[
                    {
                      required: true,
                      message:
                        "Please enter exchange rate.",
                    },
                  ]}
                >
                  <InputNumber
                    min={
                      0.000001
                    }
                    precision={6}
                    disabled={
                      !isDraft
                    }
                    style={{
                      width: "100%",
                    }}
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={10}
              >
                <Form.Item
                  name="paymentAccountId"
                  label="Payment Account"
                  rules={[
                    {
                      required: true,
                      message:
                        "Please select payment account.",
                    },
                  ]}
                >
                  <Select
                    showSearch
                    allowClear
                    loading={
                      lookupLoading
                    }
                    disabled={
                      !isDraft
                    }
                    placeholder="Select payment account"
                    optionFilterProp="label"
                    options={accounts.map(
                      (
                        account
                      ) => ({
                        value:
                          account.accountId,

                        label:
                          `${account.accountCode} - ${account.accountName}`,
                      })
                    )}
                  />
                </Form.Item>
              </Col>

              <Col
                xs={24}
                md={8}
              >
                <Form.Item
                  name="notes"
                  label="Notes"
                >
                  <TextArea
                    rows={1}
                    disabled={
                      !isDraft
                    }
                    placeholder="Notes"
                  />
                </Form.Item>
              </Col>
            </Row>
          </Form>
        </Card>

        <Card
          title="Expense Items"
          extra={
            isDraft && (
              <Button
                type="primary"
                icon={
                  <PlusOutlined />
                }
                onClick={
                  addItem
                }
              >
                Add Item
              </Button>
            )
          }
          style={{
            marginBottom: 20,
          }}
        >
          <Table
            rowKey="key"
            columns={
              itemColumns
            }
            dataSource={items}
            pagination={false}
            loading={
              loading
            }
            scroll={{
              x: 1050,
            }}
            locale={{
              emptyText:
                "No expense items. Click Add Item.",
            }}
          />

          <div
            style={{
              marginTop: 20,
              display: "flex",
              justifyContent:
                "flex-end",
            }}
          >
            <Card
              size="small"
              style={{
                minWidth: 320,
              }}
            >
              <div
                style={{
                  display: "flex",
                  justifyContent:
                    "space-between",
                  alignItems:
                    "center",
                }}
              >
                <Text strong>
                  Total Amount
                </Text>

                <Text
                  strong
                  style={{
                    fontSize: 20,
                  }}
                >
                  {totalAmount.toLocaleString(
                    undefined,
                    {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    }
                  )}
                </Text>
              </div>
            </Card>
          </div>
        </Card>

        {isPosted && (
          <Card
            size="small"
            style={{
              marginBottom: 20,
            }}
          >
            <Text strong>
              This expense has been
              POSTED and can no
              longer be edited.
            </Text>
          </Card>
        )}

        {isCancelled && (
          <Card
            size="small"
            style={{
              marginBottom: 20,
            }}
          >
            <Text strong>
              This expense has been
              CANCELLED.
            </Text>
          </Card>
        )}
      </div>
    </>
  );
}
