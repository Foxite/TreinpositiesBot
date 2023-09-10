import {Injectable} from "@angular/core";
import {LevelInfo} from "../../models/models";

//@Injectable()
export abstract class LevelsService {
  abstract getLevelTree(rootLevelId: string): Promise<LevelInfo | null>;
}

@Injectable()
export class MockLevelService extends LevelsService {
  private levelRoots: Record<string, LevelInfo>;

  constructor() {
    super();

    this.levelRoots = {
      "346682476149866497": {
        name: "Testing server",
        children: {
          // channels with no category
          "346682476149866498": { id: "346682476149866498", name: "dev-bots" },
          "1042105527410368532": { id: "1042105527410368532", name: "general" },
          "1042105527410368533": { id: "1042105527410368533", name: "not-real-channel-list" },
          "1042105527410368534": { id: "1042105527410368534", name: "cuz-discords-a-pain" },
        }
      },
      "942881727527940127": { // sussy
        name: "Sussy server",
        children: {
          "942881728039620628": { // category
            name: "main channels",
            children: {
              "942881728039620630": { id: "942881728039620630", name: "general", },
              "943469684764864522": { id: "943469684764864522", name: "memes", },
              "943139193226412093": {
                name: "bot",
                children: {
                  // threads
                  "962393095318667305": { id: "962393095318667305", name: "thread-1" },
                }
              },
            }
          }
        }
      }
    } as any;

    const setupTree = (parent: LevelInfo | undefined, level: LevelInfo) => {
      level.parent = parent;
      if (level.parent) {
        level.path = `${level.parent.path}:${level.id}`;
      } else {
        level.path = level.id;
      }

      if (!level.children) {
        return;
      }

      for (const childId in level.children) {
        level.children[childId].id = childId;
        setupTree(level, level.children[childId]);
      }
    }

    for (const levelRootId in this.levelRoots) {
      this.levelRoots[levelRootId].id = levelRootId;
      setupTree(undefined, this.levelRoots[levelRootId]);
    }
  }

  getLevelTree(rootLevelId: string): Promise<LevelInfo | null> {
    if (this.levelRoots[rootLevelId]) {
      return Promise.resolve(this.levelRoots[rootLevelId]);
    } else {
      return Promise.resolve(null);
    }
  }
}
