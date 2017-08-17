<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:nuspec="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <xsl:output media-type="xml" encoding="utf-8" indent="yes"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="nuspec:dependencies" />
  <xsl:template match="nuspec:frameworkAssemblies" />

  <xsl:template match="nuspec:package">
    <package>
      <xsl:apply-templates select="@* | *" />
      <files>
        <file src="build\**" target="build" />
      </files>
    </package>
  </xsl:template>
</xsl:stylesheet>
