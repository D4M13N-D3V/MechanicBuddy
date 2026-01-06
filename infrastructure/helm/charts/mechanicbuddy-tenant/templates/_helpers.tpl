{{/*
Expand the name of the chart.
*/}}
{{- define "mechanicbuddy-tenant.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "mechanicbuddy-tenant.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- printf "tenant-%s" .Values.tenant.id | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "mechanicbuddy-tenant.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "mechanicbuddy-tenant.labels" -}}
helm.sh/chart: {{ include "mechanicbuddy-tenant.chart" . }}
{{ include "mechanicbuddy-tenant.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
mechanicbuddy.app/tenant-id: {{ .Values.tenant.id | quote }}
mechanicbuddy.app/tenant-tier: {{ .Values.tenant.tier | quote }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "mechanicbuddy-tenant.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-tenant.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
API selector labels
*/}}
{{- define "mechanicbuddy-tenant.api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-tenant.name" . }}-api
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: api
{{- end }}

{{/*
Web selector labels
*/}}
{{- define "mechanicbuddy-tenant.web.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-tenant.name" . }}-web
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: web
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "mechanicbuddy-tenant.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "mechanicbuddy-tenant.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Database connection string
*/}}
{{- define "mechanicbuddy-tenant.databaseConnectionString" -}}
{{- $host := printf "%s-postgres-rw" (include "mechanicbuddy-tenant.fullname" .) -}}
{{- $port := "5432" -}}
{{- $database := .Values.postgresql.database -}}
{{- $user := .Values.postgresql.username -}}
Host={{ $host }};Port={{ $port }};Database={{ $database }};Username={{ $user }};Password=$(DB_PASSWORD)
{{- end }}

{{/*
Default domain for tenant
*/}}
{{- define "mechanicbuddy-tenant.defaultDomain" -}}
{{- if .Values.domains.default }}
{{- .Values.domains.default }}
{{- else }}
{{- printf "%s.%s" .Values.tenant.id .Values.domains.baseDomain }}
{{- end }}
{{- end }}

{{/*
All hosts for ingress (default + custom)
*/}}
{{- define "mechanicbuddy-tenant.allHosts" -}}
{{- $hosts := list (include "mechanicbuddy-tenant.defaultDomain" .) }}
{{- range .Values.domains.custom }}
{{- $hosts = append $hosts . }}
{{- end }}
{{- $hosts | toJson }}
{{- end }}

{{/*
PostgreSQL cluster name
*/}}
{{- define "mechanicbuddy-tenant.postgresName" -}}
{{- printf "%s-postgres" (include "mechanicbuddy-tenant.fullname" .) }}
{{- end }}

{{/*
Secret name for database credentials
*/}}
{{- define "mechanicbuddy-tenant.dbSecretName" -}}
{{- printf "%s-postgres-app" (include "mechanicbuddy-tenant.fullname" .) }}
{{- end }}

{{/*
Secret name for JWT and app secrets
*/}}
{{- define "mechanicbuddy-tenant.appSecretName" -}}
{{- printf "%s-app-secrets" (include "mechanicbuddy-tenant.fullname" .) }}
{{- end }}
