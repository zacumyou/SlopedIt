const path = require("path");
const { BannerPlugin } = require("webpack");
module.exports = {
  mode: "production",
  entry: { SlopedIt: "./src/index.tsx" },
  externalsType: "window",
  externals: { react: "React", "react-dom": "ReactDOM", "cs2/modding": "cs2/modding", "cs2/api": "cs2/api", "cs2/l10n": "cs2/l10n" },
  module: { rules: [
    { test: /\.tsx?$/, use: "ts-loader", exclude: /node_modules/ },
    { test: /\.svg$/, type: "asset/resource", generator: { filename: "images/SlopedIt/[name][ext]" } }
  ] },
  resolve: { extensions: [".tsx", ".ts", ".js"] },
  output: { path: path.resolve(__dirname, "../local/SlopedIt"), filename: "[name].mjs", library: { type: "module" }, publicPath: "coui://ui-mods/" },
  plugins: [new BannerPlugin({ banner: "Cities: Skylines II UI Module\n\nId: SlopedIt\nAuthor: zacum\nVersion: 1.1.1\nDependencies:", stage: Infinity })],
  experiments: { outputModule: true }
};


