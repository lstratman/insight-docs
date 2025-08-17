import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleIndexedAccessType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let indexedAccessTypeNode = node as ts.IndexedAccessTypeNode;

    return <TypeScriptIndexedAccessType> {
        kind: 'indexedAccess',
        indexType: registerTypeNodeForReflection(indexedAccessTypeNode.indexType, typeChecker),
        objectType: registerTypeNodeForReflection(indexedAccessTypeNode.objectType, typeChecker)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.IndexedAccessType, handleIndexedAccessType);