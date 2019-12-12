import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"
import { SeasonContext } from "./SeasonContext";

export default class ZoneInfoQuery extends Component {
  static contextType = SeasonContext;
  render() {
    const { zoneId, children } = this.props;
    const season = this.context;

    return (
      <Query query={GET_ZONEINFO} variables={{season, zoneId}}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_ZONEINFO = gql`
query zoneInfo($season: Int, $zoneId: Int!) {
  zoneById(id: $zoneId) {
    id,
    name,
    killmailsByZoneId(condition: {season: $season}) {
      totalCount
    }
  }
}
`;
