import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleOptionalType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let optionalTypeNode = node as ts.OptionalTypeNode;
    return registerTypeNodeForReflection(optionalTypeNode.type, typeChecker);
}

registerNodeTypeHandler(ts.SyntaxKind.OptionalType, handleOptionalType);