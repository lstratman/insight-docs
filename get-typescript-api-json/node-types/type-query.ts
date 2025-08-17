import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleTypeQuery(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let typeQueryNode = node as ts.TypeQueryNode;

    return <TypeScriptTypeQueryType> {
        kind: 'typeQuery',
        type: registerTypeNodeForReflection(typeQueryNode.exprName, typeChecker)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.TypeQuery, handleTypeQuery);