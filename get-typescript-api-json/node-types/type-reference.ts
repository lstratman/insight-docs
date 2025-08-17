import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleTypeReference(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let typeReferenceNode = node as ts.TypeReferenceNode;
    let typeReference = registerTypeNodeForReflection(typeReferenceNode.typeName, typeChecker) as TypeScriptReferenceType;

    if (typeReferenceNode.typeArguments) {
        typeReference.typeArguments = [];

        for (let typeReferenceArgument of typeReferenceNode.typeArguments) {
            typeReference.typeArguments.push(registerTypeNodeForReflection(typeReferenceArgument, typeChecker));
        }
    }

    return typeReference;
}

registerNodeTypeHandler(ts.SyntaxKind.TypeReference, handleTypeReference);