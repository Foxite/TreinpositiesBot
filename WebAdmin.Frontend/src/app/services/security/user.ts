import {RootLevelInfo} from "../../models/models";

export interface User {
	name: string;
  rootLevels: Record<string, RootLevelInfo>;
}
