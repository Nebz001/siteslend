// Mirrors AssetUnitDto and PagedResult<T> from the API. The API sends enums as text.
export type AssetCondition = 'Good' | 'Fair' | 'Damaged';

export type AssetState = 'Available' | 'Reserved' | 'Assigned' | 'UnderRepair' | 'Archived';

export interface Asset {
  id: string;
  assetTag: string;
  categoryName: string;
  description: string | null;
  condition: AssetCondition;
  state: AssetState;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
