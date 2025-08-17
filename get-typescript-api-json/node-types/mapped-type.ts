import ts from 'typescript';
import { getPropertyName, registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleMappedType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let mappedTypeNode = node as ts.MappedTypeNode;
    let mappedType: TypeScriptMappedType = {
        kind: 'mappedType',
        typeParameter: {
            name: getPropertyName(mappedTypeNode.typeParameter.name)
        }
    };

    if (mappedTypeNode.typeParameter.constraint) {
        mappedType.typeParameter.constraint = registerTypeNodeForReflection(mappedTypeNode.typeParameter.constraint, typeChecker);
    }

    if (mappedTypeNode.nameType) {
        mappedType.nameType = registerTypeNodeForReflection(mappedTypeNode.nameType, typeChecker);
    }

    if (mappedTypeNode.type) {
        mappedType.type = registerTypeNodeForReflection(mappedTypeNode.type, typeChecker);
    }

    return mappedType;
}

registerNodeTypeHandler(ts.SyntaxKind.MappedType, handleMappedType);