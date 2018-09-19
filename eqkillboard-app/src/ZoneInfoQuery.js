import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export default class ZoneInfoQuery extends Component {
  render() {
    const { zoneId, children } = this.props

    return (
      <Query query={GET_ZONEINFO} variables={{zoneId}}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_ZONEINFO = gql`
query zoneInfo($zoneId: Int!) {
  zoneById(id: $zoneId) {
    id,
    name,
    killmailsByZoneId {
      totalCount
    }
  }
}
`;
