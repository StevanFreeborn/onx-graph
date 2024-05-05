export type FormFieldState = {
  value: string;
  errorMessage: string;
};

const FieldEdgeMultiplicity = {
  SingleSelect: 0,
  MultiSelect: 1,
} as const;

type FieldEdgeMultiplicity = (typeof FieldEdgeMultiplicity)[keyof typeof FieldEdgeMultiplicity];

const FieldEdgeStatus = {
  Enabled: 0,
  Disabled: 1,
  Invalid: 2,
} as const;

type FieldEdgeStatus = (typeof FieldEdgeStatus)[keyof typeof FieldEdgeStatus];

const FieldEdgeType = {
  Reference: 500,
} as const;

type FieldEdgeType = (typeof FieldEdgeType)[keyof typeof FieldEdgeType];

type FieldEdge = {
  multiplicity: FieldEdgeMultiplicity;
  referencedAppId: number;
  id: number;
  appId: number;
  name: string;
  type: FieldEdgeType;
  status: FieldEdgeStatus;
  isRequired: boolean;
  isUnique: boolean;
};

type AppNode = {
  id: number;
  name: string;
};

export type Graph = {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  status: GraphStatus;
  nodes: AppNode[];
  edgesMap: Record<string, FieldEdge[]>;
};

export const GraphStatus = {
  NotBuilt: 0,
  Building: 1,
  Built: 2,
} as const;

export type GraphStatus = (typeof GraphStatus)[keyof typeof GraphStatus];

export type Page = {
  pageCount: number;
  pageNumber: number;
  totalPages: number;
  totalCount: number;
};

export type PageWithData<T> = Page & {
  data: T[];
};
