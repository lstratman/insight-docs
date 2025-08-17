import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleTypeOperator(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let typeOperatorNode = node as ts.TypeOperatorNode;

    return <TypeScriptTypeOperatorType> {
        kind: 'typeOperator',
        operator: typeOperatorNode.operator === ts.SyntaxKind.KeyOfKeyword ? 'keyof' : typeOperatorNode.operator === ts.SyntaxKind.ReadonlyKeyword ? 'readonly' : 'unique',
        type: registerTypeNodeForReflection(typeOperatorNode.type, typeChecker)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.TypeOperator, handleTypeOperator);