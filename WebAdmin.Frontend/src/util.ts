import {LevelInfo} from "./app/models/models";

export function getLevelFromPath(levelRoot: LevelInfo, path: string): LevelInfo {
  const levelPathSplit = path.split(':');

  if (levelPathSplit[0] !== levelRoot.id) {
    throw new Error("Attempting to set selected level with mismatched root");
  }

  let first = true;
  let level = levelRoot;
  for (const pathSegment of levelPathSplit) {
    if (first) {
      first = false;
      continue;
    }

    if (!level.children) {
      throw new Error("Level path is not found in tree");
    }

    level = level.children[pathSegment];
    if (!level) {
      throw new Error("Level path is not found in tree");
    }
  }

  return level;
}

export function getPathFromLevel(level: LevelInfo) {
  let levelStack = [ level.id ]; // todo use actual stack?

  while (level.parent) {
    level = level.parent
    levelStack = [ level.id, ...levelStack ];
  }

  return levelStack.join(":");
}
