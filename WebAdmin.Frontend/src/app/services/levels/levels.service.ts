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
        id: "346682476149866497",
        name: "Testing server",
        children: {
          // channels with no category
          "346682476149866498": { id: "346682476149866498", name: "dev-bots" },
          "1042105527410368532": { id: "1042105527410368532", name: "general" },
        }
      },
      "942881727527940127": { // sussy
        id: "942881727527940127",
        name: "Sussy server",
        children: {
          "942881728039620628": { // category
            id: "942881728039620628",
            name: "main channels",
            children: {
              "942881728039620630": { id: "942881728039620630", name: "general", },
              "943469684764864522": { id: "943469684764864522", name: "memes", },
              "943139193226412093": {
                id: "943139193226412093",
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
    };

    const setupTree = (parent: LevelInfo | undefined, level: LevelInfo) => {
      level.parent = parent;
      if (!level.children) {
        return;
      }

      for (const childId in level.children) {
        setupTree(level, level.children[childId]);
      }
    }

    for (const levelRoot in this.levelRoots) {
      setupTree(undefined, this.levelRoots[levelRoot]);
    }

    console.log(this.levelRoots);
  }

  getLevelTree(rootLevelId: string): Promise<LevelInfo | null> {
    if (this.levelRoots[rootLevelId]) {
      return Promise.resolve(this.levelRoots[rootLevelId]);
    } else {
      return Promise.resolve(null);
    }
  }
}
