<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/TR/WD-xsl">
<xsl:template match="/">
  <html>
  <body>
    <table spacing="4">
      <tr>
        <th>Token</th>
        <th>C_1</th>
        <th>C_2</th>
        <th>DateLast</th>
      </tr>
      <xsl:for-each select="TGPTokens/tblTokens">
      <tr>
        <td><xsl:value-of select="Token"/></td>
        <td><xsl:value-of select="C_1"/></td>
        <td><xsl:value-of select="C_2"/></td>
        <td><xsl:value-of select="DateLast"/></td>
      </tr>
      </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>
</xsl:stylesheet>
