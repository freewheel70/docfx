const fs = require('fs');
const path = require('path');
const vsctm = require('vscode-textmate');
const oniguruma = require('vscode-oniguruma');
/**
 * Utility to read a file as a promise
 */
function readFile(path) {
    return new Promise((resolve, reject) => {
        fs.readFile(path, (error, data) => error ? reject(error) : resolve(data));
    })
}

const wasmBin = fs.readFileSync(path.join(__dirname, './node_modules/vscode-oniguruma/release/onig.wasm')).buffer;
const vscodeOnigurumaLib = oniguruma.loadWASM(wasmBin).then(() => {
    return {
        createOnigScanner(patterns) { return new oniguruma.OnigScanner(patterns); },
        createOnigString(s) { return new oniguruma.OnigString(s); }
    };
});

const mappings = JSON.parse(fs.readFileSync("./data/scope-grammar-file.json"));

// Create a registry that can create a grammar from a scope name.
const registry = new vsctm.Registry({
    onigLib: vscodeOnigurumaLib,
    loadGrammar: async (scopeName) => {
        debugger;
        let grammarFile = mappings[scopeName];

        if (grammarFile === undefined){
            return null;
        }

        let grammarFilePath = `./grammars/${grammarFile}`;
        const data = await readFile(grammarFilePath);
        return vsctm.parseRawGrammar(data.toString(), grammarFilePath);
    }
});

module.exports = async (code, scopeName) => {
    let tokenWithScopes = []

    return registry.loadGrammar(scopeName).then(grammar => {
        let text = code.toString().split("\n");
        let ruleStack = vsctm.INITIAL;

        for (let i = 0; i < text.length; i++) {
            const line = text[i];
            tokenWithScopes.push(
                {
                    "line": i,
                    "lineTokens": []
                }
            );
            const lineTokens = grammar.tokenizeLine(line, ruleStack);
            for (let j = 0; j < lineTokens.tokens.length; j++) {
                const token = lineTokens.tokens[j];
                tokenWithScopes[i]["lineTokens"].push(
                    {
                        "index": [token.startIndex, token.endIndex],
                        "scopes": token.scopes,
                        "content": line.substring(token.startIndex, token.endIndex)
                    }
                );
            }
            ruleStack = lineTokens.ruleStack;
        }
        return tokenWithScopes;
    });
}