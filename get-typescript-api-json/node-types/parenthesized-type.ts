import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleParenthesizedType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    return registerTypeNodeForReflection((<ts.ParenthesizedTypeNode>node).type, typeChecker);
}

registerNodeTypeHandler(ts.SyntaxKind.ParenthesizedType, handleParenthesizedType);