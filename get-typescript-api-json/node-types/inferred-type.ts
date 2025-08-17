import ts from 'typescript';
import { getPropertyName, registerNodeTypeHandler } from '../reflection.js';

function handleInferType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let inferTypeNode = node as ts.InferTypeNode;

    return <TypeScriptInferredType> {
        kind: 'inferred',
        name: getPropertyName(inferTypeNode.typeParameter.name)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.InferType, handleInferType);