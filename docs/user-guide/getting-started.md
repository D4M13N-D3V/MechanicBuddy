# Getting Started

This guide walks you through your first steps with MechanicBuddy.

## First Login

### 1. Access the Application

Open your web browser and navigate to your MechanicBuddy URL:
- For local installations: `http://localhost:3025`
- For cloud installations: Your provided URL (e.g., `https://yourworkshop.mechanicbuddy.app`)

### 2. Login Credentials

Enter your credentials:
- **Username**: Provided by your administrator (default: `admin`)
- **Password**: Provided by your administrator (default: `carcare`)

!!! warning "Change Default Password"
    If using default credentials, you'll be prompted to change your password on first login. Choose a strong password!

### 3. Change Password (If Required)

If prompted:
1. Enter your new password
2. Confirm the new password
3. Click **Change Password**

Password requirements:
- Minimum 8 characters recommended
- Mix of letters, numbers, and symbols

---

## Understanding the Dashboard

After logging in, you'll see the main dashboard:

```
┌─────────────────────────────────────────────────────────┐
│  [Logo]  MechanicBuddy                    [User Menu]   │
├─────────┬───────────────────────────────────────────────┤
│         │                                               │
│ Work    │            Work Orders List                   │
│ Orders  │   ┌─────────────────────────────────────┐    │
│         │   │ # │ Date │ Client │ Vehicle │ Status │    │
│ Clients │   ├───┼──────┼────────┼─────────┼────────┤    │
│         │   │ 1 │ ...  │ ...    │ ...     │ ...    │    │
│ Vehicles│   │ 2 │ ...  │ ...    │ ...     │ ...    │    │
│         │   └─────────────────────────────────────┘    │
│ Inventory│                                             │
│         │                                               │
│ Settings│                                               │
└─────────┴───────────────────────────────────────────────┘
```

### Navigation Sidebar

| Menu Item | Description |
|-----------|-------------|
| **Work Orders** | Manage repair jobs and service work |
| **Clients** | Customer database |
| **Vehicles** | Vehicle registry |
| **Inventory** | Spare parts and supplies |
| **Requests** | Service requests from landing page |
| **Settings** | System configuration (admin only) |
| **Profile** | Your user settings |

### User Menu

Click your name in the top right for:
- **Profile** - Update your information
- **Logout** - Sign out of the application

---

## Creating Your First Work Order

Let's walk through creating a complete work order.

### Step 1: Start a New Work Order

1. Click **Work Orders** in the sidebar
2. Click the **+ New** button
3. A new work order form appears

### Step 2: Add Client Information

**Option A: Existing Client**
1. Start typing the client name in the search box
2. Select from the dropdown suggestions
3. Their information auto-fills

**Option B: New Client**
1. Click **+ New Client**
2. Choose **Private** or **Legal/Business**
3. Fill in the details:
   - Name (First/Last for private, Company name for legal)
   - Phone number
   - Email address
   - Address (optional)
4. Click **Save**

### Step 3: Add Vehicle Information

**Option A: Existing Vehicle**
1. If client has vehicles, they appear in the dropdown
2. Select the vehicle
3. Details auto-fill

**Option B: New Vehicle**
1. Click **+ New Vehicle**
2. Enter vehicle details:
   - Registration number (license plate)
   - VIN (optional but recommended)
   - Make (e.g., Toyota)
   - Model (e.g., Corolla)
   - Year, Engine, Transmission (optional)
3. Click **Save**

### Step 4: Enter Work Details

1. **Odometer**: Enter current mileage
2. **Notes**: Describe the customer's complaint or requested service

### Step 5: Save the Work Order

Click **Save** to create the work order. You'll see:
- A work order number assigned
- The work order opens for editing

---

## Adding Services and Parts

### Create an Offer (Estimate)

1. From the work order, click **+ Create Offer**
2. An offer form opens

### Add Services

1. Click **+ Add Service**
2. Enter:
   - **Service Name**: e.g., "Oil Change"
   - **Quantity**: Usually 1
   - **Price**: Labor cost
3. Click **Add**

### Add Products (Parts)

1. Click **+ Add Product**
2. Enter:
   - **Code**: Part number (optional)
   - **Name**: e.g., "Oil Filter"
   - **Quantity**: Number needed
   - **Unit Price**: Per-unit cost
   - **Discount**: Percentage off (optional)
3. Click **Add**

!!! tip "Quick Add from Inventory"
    Type the part code or name to search your inventory. Selecting an item auto-fills price and details.

### Save the Offer

Click **Save** to save the offer.

---

## Generating an Estimate (Quote)

Once you've added services and parts:

1. Click **Issue Estimate**
2. Review the preview:
   - Check line items
   - Verify totals
   - Confirm VAT calculation
3. Click **Confirm**

The system generates a PDF estimate you can:
- **Download** for printing
- **Email** directly to the customer

### Emailing the Estimate

1. Click **Send by Email**
2. Verify customer email address
3. Add a message (optional)
4. Click **Send**

---

## Converting to a Repair Job

When the customer approves the estimate:

1. Open the work order
2. Find the offer/estimate
3. Click **Accept Estimate**

This:
- Creates a repair job
- Copies all services and products
- Allows tracking actual work done

---

## Completing Work and Invoicing

### Update Product Status

As work progresses, update part status:

| Status | Meaning |
|--------|---------|
| **Unordered** | Need to order |
| **Ordered** | On order from supplier |
| **Arrived** | Received, ready to install |
| **Installed** | Fitted to vehicle |

### Complete the Work Order

1. Review all services performed
2. Verify all parts installed
3. Click **Mark Complete**

### Generate Invoice

1. Click **Issue Invoice**
2. Select payment terms:
   - **Payment Type**: Cash, Card, or Bank Transfer
   - **Due Days**: For bank transfers (e.g., 14 days)
3. Click **Issue**

The invoice is generated with:
- Unique invoice number
- All line items
- Tax calculations
- Payment instructions

### Send Invoice

1. Click **Send by Email**
2. Verify recipient
3. Click **Send**

The customer receives the invoice PDF attached.

---

## Daily Workflow Summary

```
Morning:
├── Check new service requests
├── Review pending work orders
└── Order needed parts

During Day:
├── Create work orders for walk-ins
├── Send estimates for approval
├── Update work progress
└── Mark parts as arrived/installed

End of Day:
├── Complete finished work orders
├── Issue invoices
└── Send invoices to customers
```

---

## Tips for Efficiency

### Search Shortcuts

- Use the search bar to quickly find:
  - Work orders by number
  - Clients by name or phone
  - Vehicles by registration

### Filters

The work order list can be filtered by:
- Status (In Progress, Completed, etc.)
- Date range
- Invoice status

### Batch Operations

Select multiple work orders to:
- Export data
- Delete (with confirmation)

---

## Next Steps

- [Workshop Staff Guide](workshop-staff-guide.md) - Detailed operations guide
- [Administrator Guide](administrator-guide.md) - System configuration
