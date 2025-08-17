import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleUnionType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let unionTypeNode = node as ts.UnionTypeNode;
    let unionType: TypeScriptUnionType = {
        kind: 'union',
        types: []
    };

    for (let typeComponentNode of unionTypeNode.types) {
        unionType.types.push(registerTypeNodeForReflection(typeComponentNode, typeChecker));
    }

    return unionType;
}

registerNodeTypeHandler(ts.SyntaxKind.UnionType, handleUnionType);