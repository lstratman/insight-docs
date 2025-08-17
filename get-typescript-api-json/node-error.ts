import path from 'path';
import * as ts from 'typescript';

export default class NodeError extends Error {
    public node: ts.Node;

    constructor(message: string, node: ts.Node) {
        super(message);
        this.node = node;
    }

    formatMessage(): string {
        let sourceFile = this.node.getSourceFile();
        let lineAndCharacter = sourceFile.getLineAndCharacterOfPosition(this.node.getStart(sourceFile, false));

        return `${sourceFile.fileName.replace(/\\/g, path.sep).replace(/\//g, path.sep)}(${lineAndCharacter.line}, ${lineAndCharacter.character}): error: ${this.message}`;
    }
}