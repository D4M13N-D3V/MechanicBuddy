"use client";

import { useState } from "react";
import { Button } from "@/_components/ui/Button";
import { AddTenantDialog } from "@/_components/AddTenantDialog";

export function AddTenantButton() {
  const [dialogOpen, setDialogOpen] = useState(false);

  return (
    <>
      <Button onClick={() => setDialogOpen(true)}>Add Tenant</Button>
      <AddTenantDialog open={dialogOpen} onClose={() => setDialogOpen(false)} />
    </>
  );
}
