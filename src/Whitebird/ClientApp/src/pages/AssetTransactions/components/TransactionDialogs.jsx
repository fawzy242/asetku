import React from 'react';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import Modal from '../../../components/molecules/Modal/Modal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Input from '../../../components/atoms/Input/Input';
import Select from '../../../components/atoms/Select/Select';
import DatePickerInput from '../../../components/atoms/Input/DatePickerInput';
import NumberInput from '../../../components/atoms/Input/NumberInput';
import FileUploader from '../../../components/molecules/FileUploader/FileUploader';
import ReturnModalContent from '../../../components/molecules/ReturnModalContent/ReturnModalContent';
import PostMaintenanceModalContent from '../../../components/molecules/PostMaintenanceModalContent/PostMaintenanceModalContent';
import { TRANSACTION_TYPE_OPTIONS } from '../../../core/constants/transactionTypes';

const TransactionDialogs = ({
  // CRUD Modal
  showModal,
  editingTransaction,
  isSubmitting,
  formData,
  setFormField,
  onSubmit,
  onClose,
  // Return Modal
  showReturnModal,
  isReturnSubmitting,
  returnData,
  setReturnData,
  selectedTransaction,
  onReturnSubmit,
  onReturnCancel,
  // Post Maintenance Modal
  showPostMaintenanceModal,
  isPostMaintenanceSubmitting,
  postMaintenanceData,
  setPostMaintenanceData,
  onPostMaintenanceSubmit,
  onPostMaintenanceCancel,
  // Common
  employeeOptions,
  officeOptions,
  conditionOptions,
  maintenanceTypeOptions,
  filteredAssetOptions,
  pairedTransactionOptions,
  loadingPairedOptions,
  showEmployee,
  showOffice,
  showPaired,
  showMaintenance,
  showExpectedReturn,
  showFileUploads,
  currentTransactionId,
  existingAttachments,
  attachmentFiles,
  handleFilesChange,
  handleUploadComplete,
  handleFileDeleted,
}) => {
  return (
    <>
      {/* CRUD Modal */}
      <CrudModal
        isOpen={showModal}
        onClose={onClose}
        title={editingTransaction ? 'Edit Transaction' : 'New Transaction'}
        onSubmit={onSubmit}
        isSubmitting={isSubmitting}
        submitText={editingTransaction ? 'Update' : 'Create'}
        size="xl"
      >
        <FormSection title="Transaction Details" description="Type, asset, and parties">
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Select
              label="Transaction Type"
              value={formData.transactionType || ''}
              onChange={(e) => setFormField('transactionType')(e.target.value)}
              options={TRANSACTION_TYPE_OPTIONS}
              required
            />
            <Select
              label="Asset"
              value={formData.assetId || ''}
              onChange={(e) => setFormField('assetId')(e.target.value)}
              options={filteredAssetOptions}
              required
            />
            {showEmployee && (
              <Select
                label="Employee"
                value={formData.employeeId || ''}
                onChange={(e) => setFormField('employeeId')(e.target.value)}
                options={employeeOptions}
                required
              />
            )}
            {showOffice && (
              <Select
                label="Office"
                value={formData.officeId || ''}
                onChange={(e) => setFormField('officeId')(e.target.value)}
                options={officeOptions}
                required
              />
            )}
            {showPaired && (
              <Select
                label="Paired Transaction"
                value={formData.fromAssetTransactionId || ''}
                onChange={(e) => setFormField('fromAssetTransactionId')(e.target.value)}
                options={[{ value: '', label: loadingPairedOptions ? 'Loading...' : 'Select Paired' }, ...pairedTransactionOptions]}
                required
                disabled={loadingPairedOptions}
              />
            )}
            {showExpectedReturn && (
              <DatePickerInput
                label="Expected Loan Return Date"
                value={formData.expectedLoanReturnDate || ''}
                onChange={(e) => setFormField('expectedLoanReturnDate')(e.target.value)}
              />
            )}
          </div>
        </FormSection>

        <FormSection title="Condition & Maintenance">
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <Select
              label="Condition"
              value={formData.condition || ''}
              onChange={(e) => setFormField('condition')(e.target.value)}
              options={conditionOptions}
            />
            {showMaintenance && (
              <>
                <Select
                  label="Maintenance Type"
                  value={formData.maintenanceType || ''}
                  onChange={(e) => setFormField('maintenanceType')(e.target.value)}
                  options={maintenanceTypeOptions}
                />
                <NumberInput
                  label="Maintenance Cost"
                  value={formData.maintenanceCost}
                  onChange={(e) => setFormField('maintenanceCost')(e.target.value)}
                  prefix="Rp "
                  thousandSeparator
                  decimalScale={0}
                />
              </>
            )}
          </div>
        </FormSection>

        <FormSection title="Notes & Attachments">
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <Input
              label="Notes"
              value={formData.notes || ''}
              onChange={(e) => setFormField('notes')(e.target.value)}
              multiline
              rows={3}
            />
            {showFileUploads && (
              <FileUploader
                referenceType="AssetTransaction"
                referenceId={currentTransactionId}
                entityType="transaction"
                isAttachment={true}
                existingFiles={[...existingAttachments, ...attachmentFiles]}
                onFilesChange={handleFilesChange}
                onUploadComplete={handleUploadComplete}
                onFileDeleted={handleFileDeleted}
              />
            )}
          </div>
        </FormSection>
      </CrudModal>

      {/* Return Modal */}
      <Modal
        isOpen={showReturnModal}
        onClose={onReturnCancel}
        title="Return Asset"
        size="md"
      >
        <ReturnModalContent
          transaction={selectedTransaction}
          returnData={returnData}
          onReturnDataChange={setReturnData}
          onSubmit={onReturnSubmit}
          onCancel={onReturnCancel}
          isSubmitting={isReturnSubmitting}
          conditionOptions={conditionOptions}
        />
      </Modal>

      {/* Post Maintenance Modal */}
      <Modal
        isOpen={showPostMaintenanceModal}
        onClose={onPostMaintenanceCancel}
        title="Post Maintenance"
        size="md"
      >
        <PostMaintenanceModalContent
          transaction={selectedTransaction}
          postData={postMaintenanceData}
          onPostDataChange={setPostMaintenanceData}
          onSubmit={onPostMaintenanceSubmit}
          onCancel={onPostMaintenanceCancel}
          isSubmitting={isPostMaintenanceSubmitting}
          conditionOptions={conditionOptions}
        />
      </Modal>
    </>
  );
};

export default TransactionDialogs;