import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleArrayType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    return <TypeScriptArrayType> {
        kind: 'array',
        elementType: registerTypeNodeForReflection((<ts.ArrayTypeNode>node).elementType, typeChecker)
    }
}

registerNodeTypeHandler(ts.SyntaxKind.ArrayType, handleArrayType);