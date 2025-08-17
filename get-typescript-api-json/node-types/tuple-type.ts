import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleTupleType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let tupleTypeNode = node as ts.TupleTypeNode;
    let tupleType: TypeScriptTupleType = {
        kind: 'tuple',
        elements: []
    };

    for (let tupleTypeElement of tupleTypeNode.elements) {
        if (tupleTypeElement.kind === ts.SyntaxKind.NamedTupleMember) {
            let namedTupleTypeElement = tupleTypeElement as ts.NamedTupleMember;

            tupleType.elements.push({
                kind: 'tupleElement',
                name: namedTupleTypeElement.name.text,
                type: registerTypeNodeForReflection(namedTupleTypeElement.type, typeChecker)
            });
        }

        else {
            tupleType.elements.push({
                kind: 'tupleElement',
                type: registerTypeNodeForReflection(tupleTypeElement, typeChecker)
            });
        }
    }

    return tupleType;
}

registerNodeTypeHandler(ts.SyntaxKind.TupleType, handleTupleType);