// src/core/types/category.types.ts
import { AuditableEntity, GridRequestBase } from './common.types';

/**
 * Category entity
 */
export interface CategoryEntity extends AuditableEntity {
  categoryId: number;
  categoryCode?: string | null;
  categoryName: string;
  description?: string | null;
  parentCategoryId?: number | null;
}

/**
 * Category list view - for grid display
 */
export interface CategoryListView extends Partial<CategoryEntity> {
  categoryId: number;
  categoryCode?: string | null;
  categoryName: string;
  description?: string | null;
  parentCategoryId?: number | null;
  parentCategoryName?: string | null;
  childCount: number;
  isActive: boolean;
  createdDate: string;
  createdBy: string;
  modifiedDate?: string | null;
  modifiedBy?: string | null;
  // Grid action flags
  canEdit?: boolean;
  canDelete?: boolean;
}

/**
 * Category detail view
 */
export interface CategoryDetailView extends Partial<CategoryListView> {}

/**
 * Category create view model
 */
export interface CategoryCreateViewModel {
  categoryName: string;
  categoryCode?: string | null;
  description?: string | null;
  parentCategoryId?: number | null;
}

/**
 * Category update view model
 */
export interface CategoryUpdateViewModel extends CategoryCreateViewModel {
  isActive: boolean;
}

/**
 * Category dropdown view
 */
export interface CategoryDropdownView {
  categoryId: number;
  categoryName: string;
  categoryCode?: string | null;
  parentCategoryId?: number | null;
}

/**
 * Category grid request
 */
export interface CategoryGridRequest extends GridRequestBase {
  parentCategoryId?: number | null;
}