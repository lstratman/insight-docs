import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleIntersectionType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let intersectionTypeNode = node as ts.IntersectionTypeNode;
    let intersectionType: TypeScriptIntersectionType = {
        kind: 'intersection',
        types: []
    };

    for (let typeComponentNode of intersectionTypeNode.types) {
        intersectionType.types.push(registerTypeNodeForReflection(typeComponentNode, typeChecker));
    }

    return intersectionType;
}

registerNodeTypeHandler(ts.SyntaxKind.IntersectionType, handleIntersectionType);