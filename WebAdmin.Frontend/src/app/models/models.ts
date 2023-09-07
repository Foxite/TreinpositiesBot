// These properties are used in situations where we only want to know about the root level and we don't care about its children.
// LevelInfo is used when we do care. Note that objects returned as RootLevelInfo usually don't have a children property.
export interface RootLevelInfo {
  id: string;
  name: string;
  iconUrl: string | null;
}

export interface LevelInfo extends RootLevelInfo {
  parent?: LevelInfo;
  children: LevelInfo[];
}
