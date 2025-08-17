import type * as ts from "typescript";

declare module "typescript" {
    interface Node {
        symbol?: ts.Symbol;
        id?: number;
    }

    interface Symbol {
        id?: number;
        typeId?: number;
    }
}