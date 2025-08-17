import * as ts from 'typescript';
import { getPropertyName, registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';
import NodeError from '../node-error.js';

function handleTypeLiteral(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let typeLiteralNode = node as ts.TypeLiteralNode;
    let typeLiteralType: TypeScriptTypeLiteralType = {
        kind: 'typeLiteral'
    };

    if (typeLiteralNode.members) {
        for (let memberNode of typeLiteralNode.members) {
            if (memberNode.kind === ts.SyntaxKind.MethodSignature || memberNode.kind === ts.SyntaxKind.CallSignature) {
                if (!typeLiteralType.members) {
                    typeLiteralType.members = [];
                }

                let methodSignatureNode = memberNode as (ts.MethodSignature | ts.CallSignatureDeclaration);
                let methodName = memberNode.name ? getPropertyName(memberNode.name) : '';
                let method: TypeScriptMethod = {
                    id: 0,
                    kind: 'method',
                    name: memberNode.name ? getPropertyName(memberNode.name) : '',
                    signatures: [{
                        id: 0,
                        kind: 'signature',
                        name: methodName,
                        returnType: registerTypeNodeForReflection(methodSignatureNode.type, typeChecker)
                    }]
                };

                if (methodSignatureNode.parameters) {
                    method.signatures[0].parameters = [];

                    for (let methodParameter of methodSignatureNode.parameters) {
                        let methodParameterMetadata: TypeScriptParameter = {
                            kind: 'parameter',
                            name: getPropertyName(methodParameter.name),
                            type: registerTypeNodeForReflection(methodParameter.type, typeChecker)
                        };

                        if (methodParameter.questionToken) {
                            methodParameterMetadata.flags = {
                                isOptional: true
                            }
                        }

                        method.signatures[0].parameters.push(methodParameterMetadata);
                    }
                }

                typeLiteralType.members.push(method);
            }

            else if (memberNode.kind === ts.SyntaxKind.PropertySignature) {
                if (!typeLiteralType.members) {
                    typeLiteralType.members = [];
                }

                typeLiteralType.members.push(<TypeScriptProperty> {
                    kind: 'property',
                    name: memberNode.name ? getPropertyName(memberNode.name) : '',
                    type: registerTypeNodeForReflection((<ts.PropertySignature>memberNode).type, typeChecker)
                });
            }

            else if (memberNode.kind === ts.SyntaxKind.IndexSignature) {
                let indexSignatureNode = memberNode as ts.IndexSignatureDeclaration;

                typeLiteralType.indexSignature = {
                    id: 0,
                    kind: 'signature',
                    name: '',
                    returnType: registerTypeNodeForReflection(indexSignatureNode.type, typeChecker),
                    parameters: []
                };

                for (let indexParameter of indexSignatureNode.parameters) {
                    typeLiteralType.indexSignature.parameters.push({
                        kind: 'parameter',
                        name: getPropertyName(indexParameter.name),
                        type: registerTypeNodeForReflection(indexParameter.type, typeChecker)
                    });
                }
            }

            else if (memberNode.kind === ts.SyntaxKind.ConstructSignature) {
                let constructSignatureNode = memberNode as ts.ConstructSignatureDeclaration;

                if (!typeLiteralType.constructorMetadata) {
                    typeLiteralType.constructorMetadata = {
                        kind: 'method',
                        id: 0,
                        name: 'constructor',
                        signatures: []
                    };
                }

                let constructorSignature: TypeScriptMethodSignature = {
                    id: 0,
                    kind: 'signature',
                    name: '',
                    returnType: null,
                    parameters: []
                };

                for (let constructorParameter of constructSignatureNode.parameters) {
                    constructorSignature.parameters.push({
                        kind: 'parameter',
                        name: getPropertyName(constructorParameter.name),
                        type: registerTypeNodeForReflection(constructorParameter.type, typeChecker)
                    });
                }

                typeLiteralType.constructorMetadata.signatures.push(constructorSignature);
            }

            else {
                throw new NodeError(`Encountered unsupported type literal member type kind: ${memberNode.kind}`, memberNode);
            }
        }
    }

    return typeLiteralType;
}

registerNodeTypeHandler(ts.SyntaxKind.TypeLiteral, handleTypeLiteral);