export interface FileInfo {
  format: string;
  fileExtension: string;
  type: string;
  convertibleTo: ConvertibleFormat[];
  sections: Section[];
}

export interface ConvertibleFormat {
  name: string;
  extension: string;
}

export interface Section {
  title: string;
  category: string;
  properties?: Property[];
  items?: Item[];
}

export interface Property {
  name: string;
  value: string;
  format?: string;
}

export interface Item {
  title: string;
  properties?: Property[];
  details?: Record<string, string>;
  sections?: Section[];
}
