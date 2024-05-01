export type FormFieldState = {
  value: string;
  errorMessage: string;
};

export type Graph = {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  status: GraphStatus;
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
