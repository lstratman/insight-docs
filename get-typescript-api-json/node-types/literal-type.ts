import * as ts from 'typescript';
import { registerNodeTypeHandler } from '../reflection.js';
import NodeError from '../node-error.js';

function handleLiteralType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let literalTypeNode = node as ts.LiteralTypeNode;
    let literalType: 'number' | 'string' | 'null' | 'boolean';
    let literalValue: string;

    if (literalTypeNode.literal.kind === ts.SyntaxKind.NullKeyword) {
        literalType = 'null';
        literalValue = null;
    }

    else if (literalTypeNode.literal.kind === ts.SyntaxKind.TrueKeyword) {
        literalType = 'boolean';
        literalValue = 'true';
    }

    else if (literalTypeNode.literal.kind === ts.SyntaxKind.FalseKeyword) {
        literalType = 'boolean';
        literalValue = 'false';
    }

    else if (literalTypeNode.literal.kind === ts.SyntaxKind.StringLiteral) {
        literalType = 'string';
        literalValue = (<ts.StringLiteral>literalTypeNode.literal).text;
    }

    else if (literalTypeNode.literal.kind === ts.SyntaxKind.NumericLiteral) {
        literalType = 'number';
        literalValue = (<ts.NumericLiteral>literalTypeNode.literal).text;
    }

    else if (literalTypeNode.literal.kind === ts.SyntaxKind.PrefixUnaryExpression) {
        let prefixUnaryExpression = literalTypeNode.literal as ts.PrefixUnaryExpression;

        literalType = 'number';

        if (prefixUnaryExpression.operator === ts.SyntaxKind.PlusPlusToken) {
            literalValue = '++';
        }
        
        else if (prefixUnaryExpression.operator === ts.SyntaxKind.MinusMinusToken) {
            literalValue = '--';
        }
        
        else if (prefixUnaryExpression.operator === ts.SyntaxKind.PlusToken) {
            literalValue = '+';
        }
        
        else if (prefixUnaryExpression.operator === ts.SyntaxKind.MinusToken) {
            literalValue = '-';
        }
        
        else if (prefixUnaryExpression.operator === ts.SyntaxKind.TildeToken) {
            literalValue = '~';
        }
        
        else if (prefixUnaryExpression.operator === ts.SyntaxKind.ExclamationToken) {
            literalValue = '!';
        }

        else {
            throw new NodeError(`Encountered unsupported prefix unary operator: ${prefixUnaryExpression.operator}`, prefixUnaryExpression);
        }

        literalValue += prefixUnaryExpression.operand.getText();
    }

    else {
        throw new NodeError(`Encountered unsupported literal type: ${literalTypeNode.literal.kind}`, literalTypeNode.literal);
    }

    return <TypeScriptLiteralType> {
        kind: 'literal',
        literalType: literalType,
        literalValue: literalValue
    }
}

registerNodeTypeHandler(ts.SyntaxKind.LiteralType, handleLiteralType);