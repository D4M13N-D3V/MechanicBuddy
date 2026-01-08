# Workshop Staff Guide

This comprehensive guide covers daily operations in MechanicBuddy for service advisors, mechanics, and office staff.

## Work Orders

Work orders are the core of MechanicBuddy. Each work order tracks a single service job from start to finish.

### Work Order List

Navigate to **Work Orders** to see all work orders.

**List Columns:**
| Column | Description |
|--------|-------------|
| # | Work order number (auto-assigned) |
| Date | When work started |
| Client | Customer name |
| Vehicle | Registration and description |
| Status | Current state |
| Invoice | Invoice number if issued |

**Filtering Options:**
- **Search**: Find by work number, client name, or vehicle registration
- **Status**: Show only specific statuses
- **Date Range**: Filter by start date
- **Has Invoice**: Show only invoiced/uninvoiced

### Creating Work Orders

1. Click **+ New** in the Work Orders page
2. Fill in the form:

| Field | Required | Description |
|-------|----------|-------------|
| Client | No | Customer (can add later) |
| Vehicle | No | Vehicle being serviced |
| Odometer | No | Current mileage reading |
| Notes | No | Customer complaint or request |

3. Click **Save**

!!! tip "Quick Start"
    You can create a work order with just notes and add client/vehicle later.

### Work Order Details

Opening a work order shows:

**Header Section:**
- Work order number
- Status badge
- Client and vehicle information
- Start/completion dates

**Activities Section:**
- List of offers (estimates)
- List of repair jobs
- Invoice status

**Actions:**
- Create Offer
- Issue Invoice
- Change Status
- Delete Work Order

### Work Order Statuses

| Status | When to Use |
|--------|-------------|
| **Created** | Just started, no work done yet |
| **In Progress** | Actively being worked on |
| **Completed** | All work finished |
| **Cancelled** | Work cancelled by customer |

To change status:
1. Click **Change Status** button
2. Select new status
3. Confirm the change

---

## Offers and Estimates

An offer is a proposed list of services and parts. When issued as an estimate, it becomes a formal quote for the customer.

### Creating an Offer

1. Open a work order
2. Click **+ Create Offer**
3. Add services and products (see below)
4. Click **Save**

### Adding Services

Services represent labor charges:

1. Click **+ Add Service**
2. Enter details:

| Field | Description |
|-------|-------------|
| Name | Service description (e.g., "Brake Pad Replacement") |
| Quantity | Usually 1 (can be hours) |
| Unit | Hours, Each, etc. |
| Price | Labor charge |
| Discount | Optional percentage discount |

3. Click **Add**

**Example Services:**
- Oil Change - 1 x $50
- Brake Inspection - 1 x $30
- Wheel Alignment - 1 x $80

### Adding Products

Products are parts and materials:

1. Click **+ Add Product**
2. Enter details:

| Field | Description |
|-------|-------------|
| Code | Part number for tracking |
| Name | Part description |
| Quantity | Number needed |
| Unit | Each, Set, Liter, etc. |
| Price | Per-unit price |
| Discount | Optional percentage off |

3. Click **Add**

**Quick Add from Inventory:**
- Type part code or name
- Select from suggestions
- Details auto-fill from inventory

### Editing Line Items

To modify an existing item:
1. Click the item row
2. Edit the values
3. Changes save automatically

To remove an item:
1. Click the trash icon on the row
2. Confirm deletion

### Issuing an Estimate

Once the offer is complete:

1. Click **Issue Estimate**
2. Review the preview showing:
   - All line items
   - Subtotals
   - VAT calculation
   - Grand total
3. Click **Confirm**

The estimate is assigned a number (format: `workNumber-offerNumber`, e.g., "1234-0").

### Sending Estimates

To email an estimate:

1. Click **Send by Email**
2. Enter or verify email address
3. Customize subject (optional)
4. Add personal message (optional)
5. Click **Send**

The customer receives:
- Email with your custom message
- PDF estimate attached

### Accepting an Estimate

When customer approves:

1. Open the offer
2. Click **Accept Estimate**
3. This creates a repair job with copied items

!!! note "Estimate Accepted"
    After accepting, the offer shows "Accepted on [date]" and a linked repair job appears.

---

## Repair Jobs

Repair jobs track actual work performed.

### Created from Accepted Offers

When you accept an estimate:
- A repair job is created
- All products and services copy over
- You can modify as actual work differs

### Adding Work Not in Estimate

Sometimes you discover additional issues:

1. Open the repair job
2. Add new services or products
3. These additions appear on the final invoice

### Product Installation Status

Track part status through the workflow:

| Status | Meaning | Use When |
|--------|---------|----------|
| **Unordered** | Part needs ordering | Initial state |
| **Ordered** | Order placed with supplier | After ordering |
| **Arrived** | Part received at shop | When part arrives |
| **Installed** | Fitted to vehicle | After installation |

To update:
1. Click status dropdown on product row
2. Select new status
3. Status updates immediately

### Adding Mechanic Assignment

Track who performed each service:

1. Click mechanic dropdown on service row
2. Select the mechanic
3. Assignment saves automatically

---

## Invoicing

### When to Invoice

Create an invoice when:
- All work is completed
- Customer is ready to pay
- You need official documentation

### Issuing an Invoice

1. Open the work order
2. Click **Issue Invoice**
3. Select payment options:

| Option | Description |
|--------|-------------|
| **Payment Type** | Cash, Card, or Bank Transfer |
| **Due Days** | Days until payment due (for bank transfer) |

4. Click **Issue**

The invoice receives:
- Sequential invoice number
- Current date as issue date
- All completed work items
- VAT calculations

### Invoice Contents

The generated invoice includes:
- Your company details (from settings)
- Customer information
- Vehicle information
- Itemized services and parts
- Subtotal, VAT, and total
- Payment instructions

### Sending Invoices

1. Click **Send by Email** on the invoice
2. Verify recipient email
3. Click **Send**

### Marking as Paid

When payment is received:

1. Open the invoice
2. Click **Mark as Paid**
3. The invoice shows as paid

### Viewing/Downloading Invoices

- Click **Download PDF** to save locally
- Click **View** to preview in browser
- Use **Print** to send to printer

---

## Client Management

### Client List

Navigate to **Clients** to see all customers.

**Search Options:**
- Name (first, last, or company)
- Phone number
- Email address

### Viewing Client Details

Click a client to see:
- Contact information
- Address
- Associated vehicles
- Work order history

### Creating Clients

**Private Client (Individual):**
1. Click **+ New Client**
2. Select **Private**
3. Enter:
   - First Name
   - Last Name
   - Phone
   - Email
   - Address (optional)
4. Click **Save**

**Legal Client (Business):**
1. Click **+ New Client**
2. Select **Legal/Business**
3. Enter:
   - Company Name
   - Registration Number
   - Contact Phone
   - Email
   - Address
4. Click **Save**

### Editing Clients

1. Open client details
2. Click **Edit**
3. Modify information
4. Click **Save**

### Client Notes

Use the description field for:
- Special instructions
- Payment preferences
- Communication notes

---

## Vehicle Management

### Vehicle Registry

Navigate to **Vehicles** to see all vehicles.

**Search by:**
- Registration number (license plate)
- VIN
- Make and model

### Vehicle Information

| Field | Description |
|-------|-------------|
| Registration | License plate number |
| VIN | Vehicle Identification Number |
| Make | Manufacturer (Toyota, Honda, etc.) |
| Model | Vehicle model (Corolla, Civic, etc.) |
| Year | Manufacturing year |
| Engine | Engine description |
| Transmission | Auto, Manual, CVT, etc. |
| Body | Sedan, SUV, Truck, etc. |
| Color | Vehicle color |

### Creating Vehicles

1. Click **+ New Vehicle**
2. Enter vehicle details
3. Click **Save**

!!! tip "VIN Lookup"
    Enter the VIN and some systems can auto-fill make, model, and year.

### Vehicle-Client Association

Vehicles are linked to clients through registration:

1. Open vehicle details
2. Click **Register to Owner**
3. Select the client
4. Registration date is recorded

This creates ownership history - useful for:
- Tracking ownership changes
- Finding previous work on the vehicle
- Customer history

---

## Inventory Management

### Spare Parts List

Navigate to **Inventory** to see all parts.

**Columns:**
| Column | Description |
|--------|-------------|
| Code | Part number |
| Name | Part description |
| Price | Selling price |
| Quantity | Stock on hand |
| Storage | Location in warehouse |

### Adding Parts

1. Click **+ Add Part**
2. Enter:
   - Part code
   - Name/description
   - Purchase price
   - Selling price
   - Initial quantity
   - Storage location (optional)
3. Click **Save**

### Updating Stock

When stock changes:
1. Open the part
2. Update quantity
3. Save changes

!!! note "Automatic Updates"
    When parts are used in repair jobs, consider manually updating inventory quantities.

### Storage Locations

Organize parts by location:

1. Go to **Settings > Storages**
2. Add storage locations (e.g., "Shelf A", "Back Room")
3. Assign parts to locations

### Low Stock Alerts

Filter inventory to show low stock:
1. Use the **Low Stock** filter
2. Review items needing reorder

---

## Service Requests

Service requests come from your public landing page.

### Viewing Requests

Navigate to **Requests** to see customer inquiries.

**Request Information:**
- Customer name
- Contact details
- Vehicle information
- Requested service
- Message/notes
- Submission date

### Request Statuses

| Status | Meaning |
|--------|---------|
| **New** | Just received |
| **Contacted** | You've reached out |
| **Scheduled** | Appointment booked |
| **Completed** | Service done |
| **Cancelled** | Customer cancelled |

### Processing Requests

1. Review the request details
2. Contact the customer
3. Update status to **Contacted**
4. Schedule appointment
5. Update status to **Scheduled**
6. Create work order when customer arrives
7. Mark **Completed** when done

### Converting to Work Order

From a service request:
1. Note the customer details
2. Create a new work order
3. Add or find the client
4. Add or find the vehicle
5. Reference the request in notes

---

## Reports and Exports

### Work Order Search

Use filters to find specific work orders:
- Date range
- Status
- Invoice status
- Client

### Exporting Data

To export work order data:
1. Apply desired filters
2. Click **Export**
3. Choose format (CSV/Excel)

### Printing Lists

Use your browser's print function (Ctrl+P) on any list page.

---

## Tips and Best Practices

### Efficient Data Entry

1. **Use search**: Start typing to find existing clients/vehicles
2. **Tab between fields**: Faster than clicking
3. **Copy from estimates**: Accepted estimates auto-fill repair jobs

### Keeping Records Clean

1. **Complete work orders**: Don't leave them hanging
2. **Update statuses**: Keep statuses current
3. **Add notes**: Document unusual situations

### Customer Communication

1. **Send estimates promptly**: Customers appreciate quick quotes
2. **Follow up**: Check on pending estimates
3. **Confirm completion**: Send invoice as confirmation

### End of Day Checklist

- [ ] All completed work has invoices issued
- [ ] Pending work orders have current status
- [ ] New service requests reviewed
- [ ] Parts ordered as needed
