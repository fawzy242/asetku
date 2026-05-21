import React from 'react';
import { Grid } from '@mui/material';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import DatePickerInput from '../../components/atoms/Input/DatePickerInput';
import NumberInput from '../../components/atoms/Input/NumberInput';
import FileUploader from '../../components/molecules/FileUploader/FileUploader';

export const AssetForm = ({ formData, setFormData, editingAsset, categories, suppliers, offices, assetConditions, reload }) => {
  const categoryOptions = categories.map(c => ({ value: c.value, label: c.label }));
  const supplierOptions = [{ value: "", label: "None" }, ...suppliers.map(s => ({ value: s.value, label: s.label }))];
  const officeOptions = [{ value: "", label: "None" }, ...offices.map(o => ({ value: o.value, label: o.label }))];
  const conditionOptions = [{ value: "", label: "Select Condition" }, ...assetConditions.map(c => ({ value: c.value, label: c.label }))];
  const conditionPurchaseOptions = [
    { value: "", label: "Select" },
    { value: "1", label: "New" },
    { value: "2", label: "Second Hand" },
  ];

  return (
    <Grid container spacing={2}>
      <Grid item xs={12} sm={6}>
        <Input label="Asset Code" value={formData.assetCode || ""} onChange={e => setFormData({ ...formData, assetCode: e.target.value })} required />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Input label="Asset Name" value={formData.assetName || ""} onChange={e => setFormData({ ...formData, assetName: e.target.value })} required />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Select label="Category" value={formData.categoryId || ""} onChange={e => setFormData({ ...formData, categoryId: e.target.value })} options={categoryOptions} required />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Select label="Supplier" value={formData.supplierId || ""} onChange={e => setFormData({ ...formData, supplierId: e.target.value })} options={supplierOptions} />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Input label="Brand" value={formData.brand || ""} onChange={e => setFormData({ ...formData, brand: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Input label="Model" value={formData.model || ""} onChange={e => setFormData({ ...formData, model: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="Serial Number" value={formData.serialNumber || ""} onChange={e => setFormData({ ...formData, serialNumber: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="IMEI" value={formData.imei || ""} onChange={e => setFormData({ ...formData, imei: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="MAC Address" value={formData.macAddress || ""} onChange={e => setFormData({ ...formData, macAddress: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="Hostname" value={formData.hostname || ""} onChange={e => setFormData({ ...formData, hostname: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="IP Address" value={formData.ipAddress || ""} onChange={e => setFormData({ ...formData, ipAddress: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Input label="Invoice Number" value={formData.invoiceNumber || ""} onChange={e => setFormData({ ...formData, invoiceNumber: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <DatePickerInput label="Purchase Date" value={formData.purchaseDate || ""} onChange={e => setFormData({ ...formData, purchaseDate: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <NumberInput label="Purchase Price" value={formData.purchasePrice} onChange={e => setFormData({ ...formData, purchasePrice: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <NumberInput label="Warranty (Months)" value={formData.warrantyPeriod} onChange={e => setFormData({ ...formData, warrantyPeriod: e.target.value })} suffix=" months" decimalScale={0} min={0} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <DatePickerInput label="Warranty Expiry" value={formData.warrantyExpiryDate || ""} onChange={e => setFormData({ ...formData, warrantyExpiryDate: e.target.value })} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Select label="Condition" value={formData.assetCondition || ""} onChange={e => setFormData({ ...formData, assetCondition: e.target.value })} options={conditionOptions} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Select label="Purchase Condition" value={formData.assetConditionPurchase || ""} onChange={e => setFormData({ ...formData, assetConditionPurchase: e.target.value })} options={conditionPurchaseOptions} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Select label="Office" value={formData.officeId || ""} onChange={e => setFormData({ ...formData, officeId: e.target.value })} options={officeOptions} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <Select label="Operasional Office" value={formData.operasionalOffice ? "true" : "false"} onChange={e => setFormData({ ...formData, operasionalOffice: e.target.value === "true" })} options={[{ value: "false", label: "No" }, { value: "true", label: "Yes" }]} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <NumberInput label="Residual Value" value={formData.residualValue} onChange={e => setFormData({ ...formData, residualValue: e.target.value })} prefix="Rp " thousandSeparator={true} decimalScale={0} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <NumberInput label="Useful Life (Years)" value={formData.usefulLife} onChange={e => setFormData({ ...formData, usefulLife: e.target.value })} suffix=" years" decimalScale={0} min={1} />
      </Grid>
      <Grid item xs={12} sm={4}>
        <DatePickerInput label="Depreciation Start" value={formData.depreciationStartDate || ""} onChange={e => setFormData({ ...formData, depreciationStartDate: e.target.value })} />
      </Grid>
      <Grid item xs={12}>
        <Input label="Notes" value={formData.notes || ""} onChange={e => setFormData({ ...formData, notes: e.target.value })} multiline rows={2} />
      </Grid>
      <Grid item xs={12}>
        <FileUploader 
          referenceTable="Asset"
          referenceId={editingAsset?.assetId}
          onUploadComplete={reload}
        />
      </Grid>
    </Grid>
  );
};