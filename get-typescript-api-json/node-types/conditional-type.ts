import * as ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleConditionalType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let conditionalTypeNode = node as ts.ConditionalTypeNode;

    return <TypeScriptConditionalType> {
        kind: 'conditional',
        checkType: registerTypeNodeForReflection(conditionalTypeNode.checkType, typeChecker),
        extendsType: registerTypeNodeForReflection(conditionalTypeNode.extendsType, typeChecker),
        trueType: registerTypeNodeForReflection(conditionalTypeNode.trueType, typeChecker),
        falseType: registerTypeNodeForReflection(conditionalTypeNode.falseType, typeChecker)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.ConditionalType, handleConditionalType);