using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoicesService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_identification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                schema: "billing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "billing",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_invoice_id",
                schema: "billing",
                table: "invoice_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "idx_customer_id",
                schema: "billing",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_number",
                schema: "billing",
                table: "invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_issue_date",
                schema: "billing",
                table: "invoices",
                column: "issue_date");

            migrationBuilder.CreateIndex(
                name: "idx_status",
                schema: "billing",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_status_created_at",
                schema: "billing",
                table: "outbox_messages",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_items",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "billing");
        }
    }
}
