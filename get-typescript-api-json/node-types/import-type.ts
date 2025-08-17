import * as ts from 'typescript';
import { registerNodeTypeHandler } from '../reflection.js';
import NodeError from '../node-error.js';

function handleImportType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let importTypeNode = node as ts.ImportTypeNode;
    let importArgument: string;

    if (importTypeNode.argument.kind === ts.SyntaxKind.LiteralType && (<ts.LiteralTypeNode>importTypeNode.argument).literal.kind === ts.SyntaxKind.StringLiteral) {
        importArgument = (<ts.StringLiteral>(<ts.LiteralTypeNode>importTypeNode.argument).literal).text;
    }

    else {
        throw new NodeError(`Encountered unsupported import() argument type: ${importTypeNode.argument.kind}`, importTypeNode);
    }

    return <TypeScriptImportType> {
        kind: 'import',
        argument: importArgument
    };
}

registerNodeTypeHandler(ts.SyntaxKind.ImportType, handleImportType);